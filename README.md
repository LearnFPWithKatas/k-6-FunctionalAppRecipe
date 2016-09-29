# Kata: Recipe for Functional App

### What does this kata teach?

* Recipe for an end-to-end Functional App
* Type Driven Design
* Railway Oriented Programming
* Idiomatically add cross cutting concerns like parameter validation, logging, eventing, exception handling

### How to use this kata?

0. Speed read the references below (OK to understand very little to start with, emphasis is on building finger memory)
1. Clone repo
2. Checkout Step 0
3. Refer changes in each step, make the same changes on your own, commit
4. Repeat steps 2 to 4 above, until you have built finger memory for the changes

NOTE: Fully understanding the theory is not required to start with. Emphasis is on building finger memory.

### Steps

* Step 0. README & basic plumbing
* Step 1. The naive implementation
* Step 2. Type Driven Design: Define and use a domain model
* Step 3a. Railway Oriented Programming: Introduce notion of 2 tracks + refactor to send happy paths down the success track
* Step 3b. Railway Oriented Programming: Parameter Validation + Refactor to send non-happy paths down the failure track
* Step 3c. Railway Oriented Programming: Refactor to send exceptions down failure track + bubble up consolidated failure track messages
* Step 4. Idiomatically Add cross cutting concerns: logging, domain events

### The Kata Scenario:

There are two scenarios implemented in this kata:

**Get a customer**. The steps are:

* Validate the input params.
* Fetch the customer from the database.
* Convert the customer into a DTO. 
* Return the DTO or the appropriate error.

**Add or update a customer**. The steps are:

* Validate the input params.
* Convert the DTO into customer.
* Insert or update the customer in the database.
* If the customer's name has changed, send them a notification.
* Return OK or the appropriate error.

### References:

* [The "A recipe for a functional app" series](http://fsharpforfunandprofit.com/series/a-recipe-for-a-functional-app.html)
* [Railway Oriented Programming](http://fsharpforfunandprofit.com/rop/)
