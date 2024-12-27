# Distributed Transactions PoC

## Objective
The purpose of this project is to setup a proof of concept that will demonstrate how to leverage the dotnet ef core technology stack in order to optimize throughput into CRDB for various use cases.  The first iteration of the PoC solution is to simulate the behavior of a customer allocation engine, where multiple trade executions for a block order are stitched together and processed against a list of accounts waiting on order fulfillment.  Some workflows leverage large batch cycles and grouped transactions in order to limit database round trips and process as many records as possible in a single context.  However, we want to show that we can split up those batches through a distributed system and execute smaller transactions with higher concurrency and achieve better throughput.

## Outline
The workflow is roughly outlined in three phases

#### i) Routing
* client portfolios are evaluated for eligibility and drift
* buy / sell adjustments are decided for each position
* changes for each asset are aggregated across accounts into block orders
* orders are sent through the OMS to the market for fulfillment
* and later the post trade allocations will be received and sent for booking

#### ii) Allocations
* individual trade executions are collected from the market
* trades are aggregated back to the accounts mapped in the block order
* snapshots are taken along the way to partially fill orders with a fair market price
* allocations are then sent for routing and booking

#### iii) Booking
* back office system will reconcile account and position details
* and prepare the completed transaction for settlement

## Domain Models
We'll break the problem up with a clear separation of concerns between each step of the workflow.  To achieve this we'll define a separate database for each domain.
* **reference**: will hold a list of securities that can be traded
* **market**: will contain a time series of daily prices for securities
* **holdings**: will define customer portfolio accounts and security positions
* **orders**: will maintain a list of block orders required for rebalancing
* **trades**: will capture the trades executed against those block orders
* **allocations**: will aggregate trades and track fulfillment against block orders

## Process Flow
The first step is to setup static data that we can use to run our tests.
1) load **reference** domain with predefined security assets
2) publish daily pricing information into the **market** domain
3) generate accounts with random positions stored in the **holdings** domain

Next we'll create block orders that can be used to simuate trade executions.
1) define a process to determine eligible portfolios with criteria-based groups in **holdings**
2) enable new groups to be defined to structure blocks for intraday activity with the same criteria
3) based on a given trading day, rebalance **holdings** using the open price in the **market**
4) map protfolio **holding** accounts and **reference** securities to the group definitions for each customer **order**
5) aggregate related group positions across all security holdings into the block **orders** domain

Then we'll generate trade executions off of block orders and publish them downstream to the customer allocation process.  This will allow us focus on consumption, aggregation and distribution in the allocation phase independently of the routing phase.
1) generate executions in the **trades** domain to draw down against block **orders**
2) and leverage CDC to publish trade executions into a partitioned Kafka topic
3) consume those trade executions and persist them in the **allocations** domain
4) map each **allocation** to a custoner **order** and generate a fullfillment of the order
5) interrogate pricess in the **market** along with previous trade executions to determine a fullfilment price

With this we'll be able to test the flow with different CRDB sizes, configurations, best practices, scenarios to optimize throughput and make recommendations for an actual implementation and production deployment.

## Use Cases
We can leverage the PoC to demonstrate various use cases and outcomes defined below.

#### i) Trade Capture
We want to consume trade executions as quickly as possible, maintaining the order, and persisiting events to the database with transactional integrity.  The rate of message delivery and writing to the database should be independent, and we can control throughput by scaling the number of message sinks, application processes, and database nodes.

![Trade Capture](https://github.com/user-attachments/assets/b32e9c85-71d7-4021-aff6-bf8dc20ec660)

#### ii) Rebalancing
We want to implement a complex multi-step workflow with interprocess dependencies using an IPC protocol for guaranteed delivery.  To accomplish this we'll leverage the transactional outbox pattern, linking together independent units of work in our data pipeline with CDC messages sent to Kafka topics.

![Initialize Rebalance](https://github.com/user-attachments/assets/a5f34960-9a79-4325-9e12-3f6da8324e09)

## Running the Simulation
You can download the repository and run the simulation locally to test different scenarios and configurations that are appropriate for your own distributed transaction workload.  More information on environment setup and the steps required to run the simulation can be found on our [wiki pages](https://github.com/roachlong/distributed-transactions-poc/wiki).
