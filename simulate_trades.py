from enum import Enum
import math
import numpy as np
import psycopg
from psycopg import errors
import random
import time

class Field(Enum):
    code = 0
    asset_class = 1
    symbol = 2
    date = 3
    direction = 4
    destination = 5
    order_type = 6
    restriction = 7
    amount = 8
    needed = 9
    high = 10
    low = 11

class Simulate_trades:

    def __init__(self, args: dict):
        # args is a dict of string passed with the --args flag
        # user passed a yaml/json, in python that's a dict object
        self.market_date = args.get("market_date", "2024-11-14")
        self.batch_size: int = int(args.get("batch_size", 128))
        self.adhoc_rate: int = int(args.get("adhoc_rate", 1))
        self.amend_rate: int = int(args.get("amend_rate", 1))
        self.cancel_rate: int = int(args.get("cancel_rate", 1))

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
        
        # logic to partition the list of block orders across the total threads
        # but the math has to work out so that the partitions don't overlap and
        # if the number of iterations exceeds to the size of the partition then stop
        with conn.cursor() as cur:
            # query how many block orders are available for the given market date
            sql = """
            select count("Id") from orders."BlockOrders"
            where cast("Date" as date) = '{0}';
            """.format(self.market_date)
            num_block_orders = cur.execute(sql).fetchone()[0]

            # then calculate the offset and limit to partition the block orders
            limit = math.ceil(num_block_orders / total_thread_count)
            offset = id * limit
            sql = """
            with orders as (
                select "Code", "AssetClass", "Symbol", "Date", "Direction",
                       "Destination", "Type", "Restriction", "Amount", "Needed"
                from orders."BlockOrders"
                where cast("Date" as date) = '{0}'
                offset {1} limit {2}
            )
            select o.*, p."High", p."Low"
            from market."Prices" p, orders o
            where p."AssetClass" = o."AssetClass"
              and p."Symbol" = o."Symbol"
              and cast(p."Date" as date) = cast(o."Date" as date);
            """.format(self.market_date, offset, limit)
            self.block_orders = [list(r) for r in cur.execute(sql).fetchall()]

        print(f"***** setup completed for thread {id} with {len(self.block_orders)} block orders")



    # the loop() function returns a list of functions
    # that dbworkload will execute, sequentially.
    # Once every func has been executed, loop() is re-evaluated.
    # This process continues until dbworkload exits.
    def loop(self):
        # print(f"id: {self.id} and counter: {self.counter} LOOP called")

        # we'll get the next batch of block orders from the partition created for this thread
        if self.counter < len(self.block_orders):
            start = self.counter * self.batch_size
            end = start + self.batch_size
            if len(self.block_orders) <= end:
                self.next_batch = self.block_orders[start:]
            else:
                self.next_batch = self.block_orders[start:end]
        else:
            self.next_batch = []
        return [self.simulate]



    # conn is an instance of a psycopg connection object
    # conn is set by default with autocommit=True, so no need to send a commit message
    def simulate(self, conn: psycopg.Connection):
        print(f"### id: {self.id} and counter: {self.counter} SIMULATE called for next batch of {len(self.next_batch)} orders")
        # if the next batch of block orders is empty then don't do anything
        if not self.next_batch:
            return
        
        seq_num = 0
        # otherwise we can simulate trades against each block order
        # until the quantity needed is fully exhausted for all orders
        while True:
            seq_num += 1
            adhoc_trades, amend_trades, cancel_trades, execute_trades = \
                zip(*map(lambda o: self.get_trade(conn, o, seq_num), self.next_batch))
            
            adhoc_trades = [t for t in adhoc_trades if t]
            amend_trades = [t for t in amend_trades if t]
            cancel_trades = [t for t in cancel_trades if t]
            execute_trades = [t for t in execute_trades if t]

            if (not adhoc_trades and not amend_trades and
                not cancel_trades and not execute_trades):
                break
            else:
                if adhoc_trades:
                    sql = """
                    INSERT INTO \"AdHocTrades\" (
                        \"BlockOrderCode\", \"BlockOrderSeqNum\", \"AssetClass\",
                        \"Symbol\", \"Date\", \"Direction\", \"Destination\",
                        \"Type\", \"Restriction\", \"Amount\", \"Price\",
                        \"AccountNum\", \"PositionId\"
                    )
                    """
                    if not self.execute(conn, sql, adhoc_trades):
                        print(f"TRADE CREATION FAILED FOR: {adhoc_trades}")

                if amend_trades:
                    sql = """
                    INSERT INTO \"ReplacedTrades\" (
                        \"BlockOrderCode\", \"BlockOrderSeqNum\", \"AssetClass\",
                        \"Symbol\", \"Date\", \"Direction\", \"Destination\",
                        \"Type\", \"Restriction\", \"NewDestination\",
                        \"NewType\", \"NewRestriction\", \"NewAmount\"
                    )
                    """
                    if not self.execute(conn, sql, amend_trades):
                        print(f"TRADE AMENDMENT FAILED FOR: {amend_trades}")
                
                if cancel_trades:
                    sql = """
                    INSERT INTO \"BustedTrades\" (
                        \"BlockOrderCode\", \"BlockOrderSeqNum\", \"AssetClass\",
                        \"Symbol\", \"Date\", \"Direction\", \"Destination\",
                        \"Type\", \"Restriction\", \"CancelledAmount\"
                    )
                    """
                    if not self.execute(conn, sql, cancel_trades):
                        print(f"TRADE CANCELATION FAILED FOR: {cancel_trades}")
                
                if execute_trades:
                    sql = """
                    INSERT INTO \"ExecutedTrades\" (
                        \"BlockOrderCode\", \"BlockOrderSeqNum\", \"AssetClass\",
                        \"Symbol\", \"Date\", \"Direction\", \"Destination\",
                        \"Type\", \"Restriction\", \"Amount\", \"Price\"
                    )
                    """
                    if not self.execute(conn, sql, execute_trades):
                        print(f"TRADE EXECUTION FAILED FOR: {execute_trades}")

        self.counter += 1
    


    def get_trade(self, conn: psycopg.Connection, order, seq_num: int):
        adhoc_data = []
        amend_data = []
        cancel_data = []
        execute_data = []

        if order[Field.needed.value] > 0:
            data = [
                order[Field.code.value],
                seq_num,
                order[Field.asset_class.value],
                order[Field.symbol.value],
                order[Field.date.value],
                order[Field.direction.value],
                order[Field.destination.value],
                order[Field.order_type.value],
                order[Field.restriction.value]
            ]

            if self.adhoc_rate >= random.randint(1, 100):
                # create an adhoc trade for one of the accounts
                adhoc_data = data
                trade_amount = random.randint(1, 5) * 100
                adhoc_data += [trade_amount]
                low = order[Field.low.value]
                high = order[Field.high.value]
                adhoc_data += [round(random.uniform(low, high), 2)]

                with conn.cursor() as cur:
                    # query for a random position related to this block order
                    sql = """
                    select "AccountNum", "PositionId"
                    from orders."CustomerOrders"
                    where "AssetClass" = '{0}'
                      and "Symbol" = '{1}'
                      and cast("Date" as date) = '{2}'
                      and "Direction" = {3}
                    order by random()
                    limit 1;
                    """.format(order[Field.asset_class.value],
                               order[Field.symbol.value],
                               order[Field.date.value],
                               order[Field.direction.value])
                    account_info = cur.execute(sql).fetchone()
                adhoc_data += [account_info[0]]
                adhoc_data += [account_info[1]]
            
            elif self.amend_rate >= random.randint(1, 100):
                # create a trade to replace some of the order details
                amend_data = data
                new_destination = random.randint(-10, 10)
                if new_destination >= 0:
                    amend_data += [new_destination]
                else:
                    amend_data += [None]
                
                new_order_type = random.randint(-4, 4)
                if new_order_type >= 0:
                    amend_data += [new_order_type]
                else:
                    amend_data += [None]

                new_restriction = random.randint(-6, 6)
                if new_restriction >= 0:
                    amend_data += [new_restriction]
                else:
                    amend_data += [None]
                
                percent_increase = np.random.normal() / 10
                if percent_increase > 0:
                    new_increase = int(order[Field.needed.value] * percent_increase)
                    order[Field.amount.value] += new_increase
                    order[Field.needed.value] += new_increase
                    amend_data += [order[Field.amount.value]]
                else:
                    amend_data += [None]
            
            elif self.cancel_rate >= random.randint(1, 100):
                # create a trade to cancel some of the original amount
                cancel_data = data
                cancel_amount = random.randint(1, order[Field.needed.value])
                order[Field.amount.value] -= cancel_amount
                order[Field.needed.value] -= cancel_amount
                cancel_data += [cancel_amount]
            
            else:
                # draw down on the order with a new trade execution
                execute_data = data
                trade_amount = min(random.randint(1, 10) * 50, order[Field.needed.value])
                order[Field.needed.value] -= trade_amount
                execute_data += [trade_amount]
                low = order[Field.low.value]
                high = order[Field.high.value]
                execute_data += [round(random.uniform(low, high), 2)]

        return adhoc_data, amend_data, cancel_data, execute_data



    def execute(self, conn: psycopg.Connection, sql, records):
        # collect the data fields from the list of records
        record_cnt = len(records)
        data = np.array(records).flatten()
        
        # and perform a multi-value insert into the table
        fields = ','.join("%s" for i in range(int(len(data) / record_cnt)))
        values = ','.join(f"({fields})" for i in range(record_cnt))
        sql = f"{sql} VALUES {values};"
        
        # note this should fail on conflict if same trade with code, seq num
        # and date exists, but will retry on serialization errors, note we don't
        # bubble those errors up because we don't want replay the entire order
        retries = 0
        max_retries = 5
        with conn.cursor() as cur:
            try:
                cur.execute(sql, tuple(data))
                return True
            except errors.SerializationFailure:
                if retries > max_retries:
                    print(f"FAILURE after {retries - 1} retries due to serialization error")
                    return False
                else:
                    retries += 1
                    print(f"RETRY number {retries} after serialization error")
                    time.sleep(0.2)
        
