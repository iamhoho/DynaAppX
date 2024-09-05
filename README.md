## DynaAppX
Tools for Dynamics CRM/365/Power Apps users.

### How to get started
The simplest way is to download the pre-packaged managed solution from the Releases section and import it into the system.
Then you can find the entry in the Aapplication ribbon, or go to the configuration page in the DynaAppx solution.
If you want to package the resources and generate the solution using the source code yourself, I will provide a tutorial at a later time.

![DynaAppx](/ReadMeSrc/DynaAppx.jpeg)
 ### Feature Introduction(The GIF demo is coming soon)
 #### AccessCheck

  In this feature, you can select a user and an entity record to view what permissions the user has on this record. The page will also display the roles and teams the user belongs to, and it supports navigation to the edit page.
 
![DynaAppx](/ReadMeSrc/AccessCheck.jpeg)
 #### InvokeFlow

  In this feature, you can select an Action or Workflow (On-Demand) and trigger the call on the page. The page provides two ways to input parameters. The first way will display the fields you need to input, and the second way allows you to use raw JSON format data as input.By scrolling down, you can also see your call records.
 
![DynaAppx](/ReadMeSrc/InvokeFlow1.jpeg)
![DynaAppx](/ReadMeSrc/InvokeFlow2.jpeg)
 #### GodPage
  In this interface, you can select any entity data you have permission to edit, even if the entity or a specific field is not configured to be displayed in the system. Before saving your data, you can also see a notification panel showing which fields you have changed.
 
![DynaAppx](/ReadMeSrc/GodPage1.jpeg)
![DynaAppx](/ReadMeSrc/GodPage2.jpeg)
 #### MetadataBrowser(Powered by Microsoft)
  This is a Microsoft-provided tool for querying metadata, which is very useful during development. You can use it to query detailed information about the system’s entities and fields.
 
