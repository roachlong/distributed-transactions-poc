from enum import Enum
import math
import psycopg
import random

class Field(Enum):
    # request info
    request_number = 0
    group_number = 1
    date = 2
    account_numbers = 3
    asset_class = 4
    symbol = 5
    # position info
    account_num = 0
    id = 1
    quantity = 2
    open = 3
    total_value = 4
    allocation = 5

class Generate_orders:

    def __init__(self, args: dict):
        # args is a dict of string passed with the --args flag
        # user passed a yaml/json, in python that's a dict object
        self.market_date = args.get("market_date", "2024-11-14")
        self.batch_size: int = int(args.get("batch_size", 128))

        # you can arbitrarely add any variables you want
        self.counter: int = 0



    # the setup() function is executed only once
    # when a new executing thread is started.
    # Also, the function is a vector to receive the excuting threads's unique id and the total thread count
    def setup(self, conn: psycopg.Connection, id: int, total_thread_count: int):
        self.id = id
        with conn.cursor() as cur:
            print(
                f"My thread ID is {id}. The total count of threads is {total_thread_count}"
            )
            print(cur.execute(f"select version()").fetchone()[0])
        
        # logic to partition the list of requests across the total threads
        # but the math has to work out so that the partitions don't overlap and
        # if the number of iterations exceeds to the size of the partition then stop
        with conn.cursor() as cur:
            # query how many security requests are available for the given market date
            sql = """
            select count("GroupNumber")
            from (
                select g."GroupNumber", unnest(g."SecuritySymbols")
                from "RebalancingGroups" g
                join "RebalancingRequests" r on r."GroupNumber" = g."GroupNumber"
                where cast(r."Date" as date) = '{0}'
            );
            """.format(self.market_date)
            num_requests = cur.execute(sql).fetchone()[0]

            # then calculate the offset and limit to partition the requests
            limit = math.ceil(num_requests / total_thread_count)
            offset = id * limit
            sql = """
            select r."RequestNumber", r."GroupNumber", r."Date", g."AccountNumbers",
                   g."AssetClass", unnest(g."SecuritySymbols") as "Symbol"
            from "RebalancingGroups" g
            join "RebalancingRequests" r on r."GroupNumber" = g."GroupNumber"
            where cast(r."Date" as date) = '{0}'
            order by "GroupNumber", "Symbol"
            offset {1} limit {2};
            """.format(self.market_date, offset, limit)
            self.requests = cur.execute(sql).fetchall()

        print(f"***** setup completed for thread {id} with {len(self.requests)} requests")



    # the loop() function returns a list of functions
    # that dbworkload will execute, sequentially.
    # Once every func has been executed, loop() is re-evaluated.
    # This process continues until dbworkload exits.
    def loop(self):
        # print(f"id: {self.id} and counter: {self.counter} LOOP called")

        # we'll get the next request from the partition created for this thread
        if (self.counter < len(self.requests)):
            self.request = self.requests[self.counter]
        else:
            self.request = None
        return [self.drift, self.aggregate]



    # conn is an instance of a psycopg connection object
    # conn is set by default with autocommit=True, so no need to send a commit message
    def drift(self, conn: psycopg.Connection):
        # print(f"### id: {self.id} and counter: {self.counter} DRIFT called for request {self.request}")
        # if the request doesn't exist then don't do anything
        if self.request is None:
            return
        
        # get the list of positions for the request with the market date price
        with conn.cursor() as cur:
            sql = """
            with portfolio as (
                select p."Id", p."AccountNum",
                       cast(p."Cash" as float) + sum(cast(x."Quantity" as float) * m."Open") as "TotalValue"
                from holdings."Portfolios" p
                join holdings."Positions" x on x."PortfolioId" = p."Id"
                join market."Prices" m on
                    m."AssetClass" = x."AssetClass"
                    and m."Symbol" = x."Symbol"
                    and cast(m."Date" as date) = '{0}'
                where p."AccountNum" in {1}
                group by p."Id", p."AccountNum"
            )
            select p."AccountNum", x."Id", x."Quantity", m."Open", p."TotalValue", x."Allocation"
            from portfolio p
            join holdings."Positions" x on
                x."PortfolioId" = p."Id"
            and x."AssetClass" = '{2}'
            and x."Symbol" = '{3}'
            join market."Prices" m on
                m."AssetClass" = x."AssetClass"
            and m."Symbol" = x."Symbol"
            and cast(m."Date" as date) = '{0}';
            """.format(self.market_date, tuple(self.request[Field.account_numbers.value]),
                       self.request[Field.asset_class.value], self.request[Field.symbol.value])
            positions = cur.execute(sql).fetchall()

        # now we can calculate the drift for each position using total portfolio value
        # and capture data required to rebalance a position as a customer order
        destination = random.randint(0, 10)
        order_type = random.randint(0, 4)
        restriction = random.randint(0, 6)
        record_cnt = 0
        data = []
        for position in positions:
            alloc = float(position[Field.allocation.value])
            price = float(position[Field.open.value])
            total_value = float(position[Field.total_value.value])
            required = int(total_value * alloc / price)
            diff = required - position[Field.quantity.value]
            if diff < 0:
                direction = 2 # Sell
            elif diff > 0:
                direction = 1 # Buy
            else: # required == quantity
                continue
            data += [
                self.request[Field.request_number.value],
                position[Field.account_num.value],
                position[Field.id.value],
                self.request[Field.asset_class.value],
                self.request[Field.symbol.value],
                self.market_date,
                direction,
                destination,
                order_type,
                restriction,
                abs(diff)
            ]
            record_cnt += 1

            # execute the next batch of inserts when count exceeds batch size
            if record_cnt >= self.batch_size:
                self.insertCustomerOrders(conn, data, record_cnt)
                record_cnt = 0
                data = []
        
        # and if there are any remaining records make sure we insert them
        if record_cnt > 0:
            self.insertCustomerOrders(conn, data, record_cnt)



    def insertCustomerOrders(self, conn: psycopg.Connection, data, record_cnt):
        # and perform a multi-value insert into the customer orders table
        ins_sql = """
        insert into "CustomerOrders" (
            "RequestNumber", "AccountNum", "PositionId",
            "AssetClass", "Symbol", "Date", "Direction",
            "Destination", "Type", "Restriction", "Quantity"
        )
        """
        fields = ','.join("%s" for i in range(int(len(data) / record_cnt)))
        values = ','.join(f"({fields})" for i in range(record_cnt))

        # note that we don't overwrite existing orders
        con_sql = "on conflict do nothing"
        with conn.cursor() as cur:
            cur.execute(f"{ins_sql} VALUES {values} {con_sql};", tuple(data))
    


    def aggregate(self, conn: psycopg.Connection):
        # print(f"### id: {self.id} and counter: {self.counter} AGGREGATE called for request {self.request}")
        # if the request doesn't exist then don't do anything
        if self.request is None:
            return
        
        # aggregate the customer orders into blocks based on the security request
        with conn.cursor() as cur:
            sql = """
            with listing as (
                select "RequestNumber", "AssetClass", "Symbol", "Date",
                       "Direction", "Destination", "Type", "Restriction",
                       sum("Quantity") as "Quantity", count("Id") as "Accounts"
                from "CustomerOrders"
                where "RequestNumber" = {0}
                and "AssetClass" = '{1}'
                and "Symbol" = '{2}'
                and cast("Date" as date) = '{3}'
                group by "RequestNumber", "AssetClass", "Symbol", "Date",
                         "Direction", "Destination", "Type", "Restriction"
            )
            insert into "BlockOrders" (
                "Code", "RequestNumber", "AssetClass", "Symbol", "Date", "Direction",
                "Destination", "Type", "Restriction", "Quantity", "Accounts"
            )
            select chr(cast(floor(random() * 26 + 65) as int)) ||
                   chr(cast(floor(random() * 26 + 65) as int)) ||
                   chr(cast(floor(random() * 26 + 65) as int)) || '-' ||
                   lpad(floor(random() * 1000000)::text, 6, '0') as "Code",
                   listing.*
            from listing
            on conflict do nothing;
            """.format(self.request[Field.request_number.value],
                       self.request[Field.asset_class.value],
                       self.request[Field.symbol.value],
                       self.market_date)
            cur.execute(sql)
        
        self.counter += 1