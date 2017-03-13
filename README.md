# README #


### What is this repository for? ###

Current application consists of:

* post code lookup front-end (Angular 2) uses api layer to retrieve and show data. 
* middle layer that returns data from the SQLServer database using Web API and uses google services for autocomplete and getting an image from google static maps.
* data Importer - data to the database is imported from the csv files provided. (data gets refreshed once in 6 month). The importer is ran manually. 
* There are applications for 3 regions Australia (master branch), New Zealand and Singapore - different data sources, different logic.

### How do I get set up? ###

You will need the following installed to get started:
* .NET core
* grunt - to install run 
```
#!cli

npm install -g grunt-cli
``` 

1) To install npm packages run from the folder /src/AMX101.Site



```
#!cli

npm install
grunt

```

Note: comment out sass config in gruntfile.js if you are getting this error:


```
#!cli

You need to have Ruby and Sass installed and in your PATH for this task to work.
More info: https://github.com/gruntjs/grunt-contrib-sass


```

2) Run the project from Visual studio.


# Running import scripts #

In Postman create post calls to import data :

* http://localhost:61150/api/import/claims
path=E:\_data\Claims.csv

* http://localhost:61150/api/import/postcodes
path=E:\_data\Postcodes.csv

* http://localhost:61150/api/import/staticclaims
path=E:\_data\StaticClaims.csv

### Continuous integration ###
Currently CI is not set up for this project. 

### Pull Requests ###
* Work in your own branch
* Commit often
* Write detailed comments in commits (inc. Jira task)
* Create Pull Requests often
* Assign Pull Requests to Garth And Steve for review


### Who do I talk to? ###
* Steve - developer
* Garth - developer
* Alex - producer
* Daria - business analyst

###Developer Notes ###
* The connection strings are stored in DataContext.cs - the database connection changes depending on the value passed to it
* The region setting is stored in /js/env.js. When set to "" the app defaults to AUS and allows the user to change it