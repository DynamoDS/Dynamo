# Sending logs from Dynamo to Forge backend
This is prototype code for implementing a pipeline of API call to FDX. The objective is to send the .dyn file compressed and encoded to Base64 to the Forge backend along with assosiated metadata using one Data Exchange container per log.
The steps that are being implemented are:
1. Create a data exchange container (returns data exchange id, collection id and schemaNamespace id)
2. Start a fulfillment (returns the fulfillment id)
3. Create a binary asset (returns an AWS upload link)
4. Upload binary content
5. Finish binary upload
6. Upload instance asset (request body includes metadata, base component and binary reference component)
7. End fulfillement

## Test
To be able to run the code, you need to pass a path to a .dyn file.

### Using Postman
You can test the same workflow the code is implementing by sequentially calling the API calls via Postman. You can use the two example collections, one interacting with the [Staging FDX environment](/FDX%20API%20Tests%20-%20STAGE%20-%203%20leg%20auth.postman_collection.json) and one for interacting with the [production FDX environment](/FDX%20API%20Tests%20-%20PROD%20-%203%20leg%20auth.postman_collection.json).
The collections has been tested with Postman version `10.18.8`.

#### Setup authentication 
To be able to use the Postman collections you need to send a Forge authorization token along with the request. To configure the Postman collection to store and use a Forge token follow these steps:
1. Import the collection on Postman.
2. Go to the collection authorization tab and fill in your Forge `Client ID` and `Client Secret`.
3. Click the `Get New Access Token` button.
4. When authorization is complete click `Proceed` and then `Use Token`.