from pymongo import MongoClient

CONNECTION_STRING = "mongodb://admin:Pa55w.rd@localhost:27017"

client = MongoClient(CONNECTION_STRING)

database = client["birds"]
