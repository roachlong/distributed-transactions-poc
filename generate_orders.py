from enum import Enum
import math
import psycopg
import random

class Field(Enum):
    id = 0
    account_num = 1
    cash = 2
    asset_class = 1
    symbol = 2
    quantity = 3
    open = 4
    value = 5
    allocation = 6

class Calculate_Eligible_Orders:

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
        
        # logic to partition the list of portfolios across the total threads
        # but the math has to work out so that the partitions don't overlap and
        # if the number of iterations exceeds to the size of the partition then stop
        with conn.cursor() as cur:
            # query how many eligible portfolios are available for the given market date
            sql = """
            select count("Id") from holdings."Portfolios"
            where "Eligible" = true
              and "OpenedOn" <= '{0}';
            """.format(self.market_date)
            num_portfolios = cur.execute(sql).fetchone()[0]

            # then calculate the offset and limit to partition the portfolios
            limit = math.ceil(num_portfolios / total_thread_count)
            offset = id * limit
            sql = """
            select "Id", "AccountNum", "Cash" from holdings."Portfolios"
            where "OpenedOn" <= '{0}'
            offset {1} limit {2};
            """.format(self.market_date, offset, limit)
            self.portfolios = cur.execute(sql).fetchall()

        print(f"***** setup completed for thread {id} with {len(self.portfolios)} portfolios")



    # the loop() function returns a list of functions
    # that dbworkload will execute, sequentially.
    # Once every func has been executed, loop() is re-evaluated.
    # This process continues until dbworkload exits.
    def loop(self):
        # print(f"id: {self.id} and counter: {self.counter} LOOP called")

        # we'll get the next portfolio from the partition created for this thread
        if (self.counter < len(self.portfolios)):
            self.portfolio = self.portfolios[self.counter]
        else:
            self.portfolio = None
        return [self.drift]



    # conn is an instance of a psycopg connection object
    # conn is set by default with autocommit=True, so no need to send a commit message
    def drift(self, conn: psycopg.Connection):
        # print(f"### id: {self.id} and counter: {self.counter} DRIFT called for portfolio {self.portfolio}")
        # if the portfolio doesn't exist then don't do anything
        if self.portfolio is None:
            return
        
        # get the list of positions for the portfolio with the market date price
        with conn.cursor() as cur:
            sql = """
            select p."Id", p."AssetClass", p."Symbol", p."Quantity",
                   m."Open", p."Value", p."Allocation"
            from holdings."Positions" p
            join market."Prices" m on
                m."AssetClass" = p."AssetClass"
                and m."Symbol" = p."Symbol"
                and cast(m."Date" as date) = '{0}'
            where "PortfolioId" = '{1}';
            """.format(self.market_date, self.portfolio[Field.id.value])
            positions = cur.execute(sql).fetchall()

        # determine cash value plus value of each position based on market open price
        total_value = float(self.portfolio[Field.cash.value])
        total_value += sum(
            position[Field.quantity.value] * position[Field.open.value]
            for position in positions
        )

        # now we can calculate the drift for each position using total portfolio value
        # and capture data required to rebalance a position as a customer order
        record_cnt = 0
        data = []
        for position in positions:
            alloc = float(position[Field.allocation.value])
            price = float(position[Field.open.value])
            required = int(total_value * alloc / price)
            diff = required - position[Field.quantity.value]
            if diff < 0:
                direction = 2 # Sell
            elif diff > 0:
                direction = 1 # Buy
            else: # required == quantity
                continue
            destination = random.randint(0, 10)
            order_type = random.randint(0, 4)
            restriction = random.randint(0, 6)
            data += [
                self.portfolio[Field.account_num.value],
                position[Field.id.value],
                position[Field.asset_class.value],
                position[Field.symbol.value],
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
                self.execute(conn, data, record_cnt)
                record_cnt = 0
                data = []
        
        # and if there are any remaining records make sure we insert them
        if record_cnt > 0:
            self.execute(conn, data, record_cnt)

        self.counter += 1



    def execute(self, conn: psycopg.Connection, data, record_cnt):
        # and perform a multi-value insert into the customer orders table
        ins_sql = """
        INSERT INTO \"CustomerOrders\" (
            \"AccountNum\", \"PositionId\", \"AssetClass\",
            \"Symbol\", \"Date\", \"Direction\", \"Destination\",
            \"Type\", \"Restriction\", \"Amount\"
        )
        """
        fields = ','.join("%s" for i in range(int(len(data) / record_cnt)))
        values = ','.join(f"({fields})" for i in range(record_cnt))

        # note that we don't overwrite existing orders
        con_sql = "ON CONFLICT DO NOTHING"
        with conn.cursor() as cur:
            cur.execute(f"{ins_sql} VALUES {values} {con_sql};", tuple(data))
        