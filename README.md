# Distributed Transactions PoC

## Objective
The purpose of this project is to setup a proof of concept that will demonstrate how to leverage the dotnet ef core technology stack in order to optimize throughput into CRDB for various use cases.  The first iteration of the PoC solution is to simulate the behavior of a customer allocation engine, where multiple trade executions for a block order are stitched together and processed against a list of accounts waiting on order fulfillment.  Some workflows leverage large batch cycles and grouped transactions in order to limit database round trips and process as many records as possible in a single context.  However, we want to show that we can split up those batches through a distributed system and execute smaller transactions with higher concurrency and achieve better throughput.

## Outline
The workflow is roughly outlined in three phases

#### i) Rebalancing
* client portfolios are evaluated for drift
* buy / sell adjustments are decided for each position
* changes for each asset are aggregated across accounts into block orders
* orders are sent to the market for fulfillment

#### ii) Allocations
* individual trade executions are collected from the block orders
* trades are aggregated back to the accounts mapped in the block order
* snapshots are taken along the way to partially fill orders with a fair market price
* orders are then sent to the OMS for processing

#### iii) Booking
* order management system will reconcile account and position details
* and send the completed transaction to back office for settlement

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

Next we'll generate trade executions off of block orders and publish them downstream to the customer allocation process.  This will simulate the rebalancing phase so that we can focus on consumption, aggregation and distribution in the allocation phase.
1) based on a given trading day, rebalance **holdings** using the open price in the **market**
2) aggregate positions across all security holdings into the block **orders** domain
3) generate executions in the **trades** domain to draw down against block **orders**
4) leverage CDC to publish trade executions into a partitioned Kafka topic

Then we can write our dotnet program to consume those trade executions and persist them in the **allocatons** domain for further processing.  And we'll be able to test the flow with different CRDB sizes, configurations, best practices, scenarios to optimize throughput and make recommendations for an actual implementation and production deployment.

## Running the Simulation
You can download the repository and run the simulation locally to test different scenarios and configurations that are appropriate for your own distributed transaction workload.  More information on environment setup and the steps required to run the simulation can be found on our [wiki pages](https://github.com/roachlong/distributed-transactions-poc/wiki).
