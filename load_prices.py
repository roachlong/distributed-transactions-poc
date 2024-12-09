from enum import Enum
import math
import os
import psycopg

class Field(Enum):
    date = 0
    open = 1
    high = 2
    low = 3
    close = 4
    volume = 5
    asset_class = 6
    symbol = 7

class Load_prices:

    def __init__(self, args: dict):
        # args is a dict of string passed with the --args flag
        # user passed a yaml/json, in python that's a dict object
        self.asset_class: int = args.get("asset_class", "Equity")
        self.batch_size: int = int(args.get("batch_size", 128))
        self.data_folder = str(args.get("data_folder", "./data/prices/20241114"))

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
        
        # doing some funny logic to partition the list of files across the total threads
        # but the math has to work out so that the partitions don't overlap and
        # if the number of iterations exceeds to the size of the partition then stop
        self.files = [name for name in os.listdir(self.data_folder) \
            if (os.path.isfile(os.path.join(self.data_folder, name)) and
                name.endswith(".csv"))]
        size = math.ceil(len(self.files) / total_thread_count)
        start = id * size
        end = (id + 1) * size
        if (id == (total_thread_count - 1)):
            self.files = self.files[start:]
        else:
            self.files = self.files[start:end]
        print(f"***** setup completed for thread {id} with {len(self.files)} files")



    # the loop() function returns a list of functions
    # that dbworkload will execute, sequentially.
    # Once every func has been executed, loop() is re-evaluated.
    # This process continues until dbworkload exits.
    def loop(self):
        # print(f"id: {self.id} and counter: {self.counter} LOOP called")

        # we'll get the next file from the partition created for this thread
        if (self.counter < len(self.files)):
            self.filename = self.files[self.counter]
        else:
            self.filename = None
        return [self.parse]



    # conn is an instance of a psycopg connection object
    # conn is set by default with autocommit=True, so no need to send a commit message
    def parse(self, conn: psycopg.Connection):
        print(f"### id: {self.id} and counter: {self.counter} PARSE called for file {self.filename}")
        # if the file doesn't exist then don't do anything
        if self.filename is None:
            return
        
        # otherwise get the file path information, symbol is the name of the file
        filepath = os.path.join(self.data_folder, self.filename)
        base = os.path.basename(filepath)
        symbol = base.split('.')[0]
        ext = base.split('.')[-1]

        # and parse the security price information if the file has a csv extension
        record_cnt = 0
        records = []
        if ext == 'csv':
            with open(filepath, 'r') as file:
                next(file) # skip the first line (header)
                for line in file:
                    record = line.strip().split(',')
                    record.append(self.asset_class)
                    record.append(symbol)
                    records.append(record)
                    record_cnt += 1

                    # execute the next batch of inserts when count exceeds batch size
                    if record_cnt >= self.batch_size:
                        self.execute(conn, records, record_cnt)
                        record_cnt = 0
                        records = []
        
        # and if there are any remaining records make sure we insert them
        if record_cnt > 0:
            self.execute(conn, records, record_cnt)
        
        self.counter += 1



    def execute(self, conn: psycopg.Connection, records, record_cnt):
        # collect the data fields from the list of records
        data = []
        for record in records:
            data += [
                record[Field.asset_class.value],
                record[Field.symbol.value],
                record[Field.date.value],
                record[Field.open.value],
                record[Field.high.value],
                record[Field.low.value],
                record[Field.close.value],
                record[Field.volume.value]
            ]
        
        # and perform a multi-value insert into the prices table
        ins_sql = """
        INSERT INTO \"Prices\" (
             \"AssetClass\", \"Symbol\", \"Date\", \"Open\",
             \"High\", \"Low\", \"Close\", \"Volume\"
        )
        """
        fields = ','.join("%s" for i in range(int(len(data) / record_cnt)))
        values = ','.join(f"({fields})" for i in range(record_cnt))

        # note that we don't overwrite existing prices
        con_sql = "ON CONFLICT DO NOTHING"
        with conn.cursor() as cur:
            cur.execute(f"{ins_sql} VALUES {values} {con_sql};", tuple(data))
        