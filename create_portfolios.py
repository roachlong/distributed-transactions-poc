import datetime
from enum import Enum
from faker import Faker
import psycopg
import random
import string

class Field(Enum):
    id = 0
    acct_num = 1
    manager_id= 2
    opened_on = 3
    cash = 4
    strategy = 5
    eligible = 6
    created_on = 7
    created_by = 8
    modified_on = 9
    modified_by = 10
    asset_class = 0
    symbol = 1
    close = 2

class Create_portfolios:

    def __init__(self, args: dict):
        # args is a dict of string passed with the --args flag
        # user passed a yaml/json, in python that's a dict object

        # you can arbitrarely add any variables you want
        self.counter: int = 0
        self.fake = Faker()



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
        return [self.get_manager, self.create_portfolio, self.add_securities]
    


    def get_manager(self, conn: psycopg.Connection):
        with conn.cursor() as cur:
            # query how many portfolio managers currently exist
            sql = """
            select count("Id") from "PortfolioManagers";
            """
            num_managers = cur.execute(sql).fetchone()[0]
        
            # create a new portfolio manager 0.1% of the time
            if num_managers == 0 or 1 >= random.randint(1, 1000):
                ins = """
                insert into "PortfolioManagers" ("Name")
                values ('{0}') returning *;
                """.format(self.fake.name())

                # and save the new portfolio manager for further processing
                try:
                    cur.execute(ins)
                except psycopg.Error as e:
                    print(f"Ignoring '{e}' error")
                num_managers += 1
            
            # and choose a random portfolio manager
            sel = """
            select * from "PortfolioManagers"
            offset cast(floor(random() * {0}) as int)
            limit 1;
            """.format(num_managers)
            self.manager = cur.execute(sel).fetchone()



    def create_portfolio(self, conn: psycopg.Connection):
        # if get portfolio manager failed then exit
        if (self.manager is None or
            len(self.manager) < 6 or
            self.manager[Field.id.value] is None
        ):
            return
        
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

            # and a random initial cash injection, strategy and eligibility
            cash = random.randint(10000, 1000000)
            strategy = random.randint(0, 4)
            eligible = random.randint(0, 100) <= 80

            ins = """
            insert into "Portfolios" ("AccountNum", "ManagerId", "OpenedOn", "Cash", "Strategy", "Eligible")
            values (%s, %s, %s, %s, %s, %s) returning *;
            """
            params = (acct_num, self.manager[Field.id.value], open_date, cash, strategy, eligible)

            # and save the new portfolio for further processing
            self.portfolio = cur.execute(ins, params).fetchone()

        # print(f"{self.counter}: created portfolio {self.portfolio}")
    


    def add_securities(self, conn: psycopg.Connection):
        # if the create portfolio failed then exit
        if (self.portfolio is None or
            len(self.portfolio) < 11 or
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
