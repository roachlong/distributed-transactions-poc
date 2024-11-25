from enum import Enum
import datetime
import psycopg
import random
import string

class Field(Enum):
    id = 0
    acct_num = 1
    opened_on = 2
    cash = 3
    created_on = 4
    created_by = 5
    modified_on = 6
    modified_by = 7
    asset_class = 0
    symbol = 1
    close = 2

class Create_portfolios:

    def __init__(self, args: dict):
        # args is a dict of string passed with the --args flag
        # user passed a yaml/json, in python that's a dict object

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



    # the loop() function returns a list of functions
    # that dbworkload will execute, sequentially.
    # Once every func has been executed, loop() is re-evaluated.
    # This process continues until dbworkload exits.
    def loop(self):
        # print(f"id: {self.id} and counter: {self.counter} LOOP called")
        return [self.create_portfolio, self.add_securities]
    


    def create_portfolio(self, conn: psycopg.Connection):
        self.counter += 1
        self.portfolio = None

        # open a new portfolio with a quasi-unique account number
        acct_num = ''.join(random.choice(string.ascii_uppercase) for _ in range(4))
        acct_num += '-' + str(abs(hash(datetime.datetime.now())))[:6]
        acct_num += '-' + f'{self.id % 100:02}'
        acct_num += '-' + f'{self.counter % 10000:04}'

        with conn.cursor() as cur:
            # and a random open date chosen from available market prices
            sql = """
            select "Date" from market."Prices"
            offset (select cast(floor(random() *
                   (select cast(count(*) as float) from market."Prices")) as int))
            limit 1;
            """
            open_date = cur.execute(sql).fetchone()[0]

            # and a random initial cash injection and strategy
            cash = random.randint(10000, 1000000)
            strategy = random.randint(0, 4)

            ins = """
            insert into "Portfolios" ("AccountNum", "OpenedOn", "Cash", "Strategy")
            values (%s, %s, %s, %s) returning *;
            """
            params = (acct_num, open_date, cash, strategy)

            # and save the new portfolio for further processing
            self.portfolio = cur.execute(ins, params).fetchone()

        # print(f"{self.counter}: created portfolio {self.portfolio}")
    


    def add_securities(self, conn: psycopg.Connection):
        # if the create portfolio failed then exit
        if (self.portfolio is None or
            len(self.portfolio) < 8 or
            self.portfolio[Field.id.value] is None
        ):
            return
        
        with conn.cursor() as cur:
            # get a list of securites available in the market on the portfolio open date
            opened_on = self.portfolio[Field.opened_on.value]
            open_day = opened_on.strftime('%Y-%m-%d')
            next_day = (opened_on + datetime.timedelta(days=1)).strftime('%Y-%m-%d')
            sql = """
            select "AssetClass", "Symbol", "Close" from market."Prices"
            where "Date" between date '{0}' and date '{1}'
            """.format(open_day, next_day)
            securities = cur.execute(sql).fetchall()

            # and pick a random number of securities
            random.shuffle(securities)
            securities = securities[:random.randint(1, len(securities))]

            # and then create empty positions for the portfolio
            positions = len(securities)
            avail_alloc = 100
            data = []
            for security in securities:
                alloc = random.randint(0, int(avail_alloc / positions))
                positions -= 1
                avail_alloc -= alloc
                data += [
                    security[Field.asset_class.value],
                    security[Field.symbol.value],
                    0,
                    security[Field.close.value],
                    alloc,
                    self.portfolio[Field.id.value]
                ]
            
            # and execute a single multi-value insert to store the positions
            ins = """
            insert into "Positions" (
                "AssetClass", "Symbol", "Quantity",
                "Price", "Allocation", "PortfolioId"
            )
            """
            record_cnt = len(securities)
            fields = ','.join("%s" for i in range(int(len(data) / record_cnt)))
            values = ','.join(f"({fields})" for i in range(record_cnt))
            cur.execute(f"{ins} VALUES {values};", tuple(data))
