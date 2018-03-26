LsbServerless is a quick demo I whipped up for my "Go Serverless with Azure Functions, Logic Apps and EventGrid" session at Philly Code Camp 2018.1.  To run the demo (and to get the full experience) please follow the script, below.  For more info, check out https://squideyes.com/2018/03/24/campity-do-dah/

* LOG INTO AZURE PORTAL
 
* CREATE A FREE SENDGRID ACCOUNT THROUGH THE AZURE PORTAL
 
* CREATE AND SAVE A SENDGRID API KEY 
 
* OPEN CLOUD SHELL

* CREATE A RESOURCE GROUP 
    * az group create --name **RESOURCEGROUPNAME** --location **LOCATION**

* CREATE TOPIC
    * az eventgrid topic create --name **TOPICNAME** --location **LOCATION** --resource-group **RESOURCEGROUPNAME**
 
* SAVE ENDPPOINT FOR LATER
 
* LIST KEYS
    * az eventgrid topic key list --name **TOPICNAME** --resource-group **RESOURCEGROUPNAME**

* SAVE KEY1 FOR LATER

* UPDATE EVENTPUBLISHER APPSETTTINGS KEY TO KEY1

* IF SOLUTION WAS NEWLY FETCHED FROM GITHUB
    * Add local.settings.json file (see MiscFiles folder)

* RUN SOLUTION 
    * NewEmployeeApp should be your Startup Project

* RUN NGROK
    * ngrok http -host-header=localhost 7071

* GRAB INSTANCE ID FROM NGROK
    * Be sure to update following endpoint after doing so

* CREATE SUBSCRIPTION
    * az eventgrid event-subscription create --name engineering-sub  --resource-group **RESOURCEGROUPNAME**  --topic-name **TOPICNAME**  --subject-ends-with engineering  --included-event-type employeeAdded --endpoint **ENDPOINT**/api/NewEmployeeHandler

* ALTERNATELY, IF FLUBBED, UPDATE ENDPOINT

* CREATE STORAGE ACCOUNT
    * az storage account create  --location **LOCATION**  --name **STORAGEACCOUNTNAME** --resource-group **RESOURCEGROUPNAME** --sku Standard_LRS

* CREATE A LOGIC APP
    * Be sure to add it to the **RESOURCEGROUPNAME** resource group and call it **LOGICAPPNAME** in **LOCATION**

* ADD EVENTGRID TO LOGIC APP
    * Subscription:  Microsoft Internal Consumption
    * ResourceType:  Microsoft.EventGrid.Topics
    * Resource Name: **TOPICNAME**


* ADD JSON PARSER TO LOGIC APP
    * Content: Data Object
    * Sample Payload:
{
"properties":{
"EmployeeEmail":{
"type":"string"
},
"EmployeeId":{
"type":"string"
},
"EmployeeName":{
"type":"string"
},
"ManagerEmail":{
"type":"string"
},
"ManagerId":{
"type":"string"
},
"ManagerName":{
"type":"string"
}
},
"type":"object"
}

* ADD A FOREACH
    * Select output for previous step: ***Body***

* ADD A SENDGRID SEND EMAIL (V2) TO FOREACH
    * Key: **SENDGRIDAPIKEY**
    * From: **FROMEMAIL**
    * To: ***ManagerEmail***
    * Subject: New Employee: ***EmployeeName***
    * Email Body: Please onboard ***EmployeName*** (Email: ***EmployeeEmail***, ID: ***EmployeeId***)


