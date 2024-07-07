
<script setup>
import { daxHelper } from '../daxHelper'

function openMetadataBrowser() {
    window.open(daxHelper.getCrmUrl() + "/WebResources/dax_/metadatabrowser/MetaDataBrowser.htm", "_blank", "height=600,width=1024,status=no,toolbar=no,menubar=no,resizable=yes");
}

function openEntityMetadataBrowser() {
    window.open(daxHelper.getCrmUrl() + "/WebResources/dax_/metadatabrowser/EntityMetaDataBrowser.htm", "_blank", "height=600,width=1024,status=no,toolbar=no,menubar=no,resizable=yes");
}
</script>

<template>
    <!DOCTYPE html>
    <html>

    <head>
        <meta http-equiv="X-UA-Compatible" content="IE=10" />
        <title>Metadata Browser Configuration Page</title>


    </head>

    <body>
        <h1>Metadata Browser Tool</h1>
        <p>The Metadata Browser has two pages: Metadata Browser &amp; Entity Metadata Browser.</p>
        <h2>Metadata Browser</h2>
        <p><b>Metadata Browser</b> displays a master/detail view of all the entities in the system.</p>
        <button :onclick="openMetadataBrowser">Open Metadata Browser</button>
        <p> With this page you can:</p>
        <ul>
            <li>Select an entity to view using the <em>Entity Metadata Browser</em></li>
            <li>Open the selected entity form in the default organization if the entity supports this</li>
            <li>Perform a text search to filter displayed entities using the following entity properties: SchemaName,
                LogicalName, DisplayName, ObjectTypeCode, or MetadataId</li>
            <li>Filter entities by any entity property values</li>
            <li>Filter the properties displayed for the selected entity</li>
        </ul>
        <h2>Entity Metadata Browser</h2>
        <p><b>Entity Metadata Browser</b> displays details about a selected entity.</p>
        <button :onclick="openEntityMetadataBrowser">Open Entity Metadata Browser</button>
        <p>With this page you can:</p>
        <ul>
            <li>View all the properties for the entity and filter the properties displayed</li>
            <li>Open the selected entity edit form in the default organization if the entity supports this</li>
            <li>Change the entity that you want to view</li>
            <li>
                View the entity attributes in a master/detail view. With this view you can:<ul>
                    <li>Open the selected attribute form in the default organization if the attribute supports this</li>
                    <li>Perform a text search to filter displayed attributes using the following attribute
                        properties:SchemaName, LogicalName, DisplayName, or MetadataId</li>
                    <li>Filter attributes by any attribute property values</li>
                    <li>Filter the properties displayed for the selected attribute</li>
                </ul>
            </li>
            <li>
                View the entity alternate keys in a master/detail view. With this view you can:<ul>
                    <li>Open the selected key form in the default organization.</li>
                </ul>
            </li>
            <li>
                View entity relationships. With these views you can:<ul>
                    <li>Open the selected relationship form in the default organization if the relationship supports this
                    </li>
                    <li>Perform a text search to filter displayed relationships using values relevant to the type of
                        relationship</li>
                    <li>Filter the relationship by any relationship value</li>
                </ul>
            </li>
            <li>
                View entity privileges. With this view you can:
                <ul>
                    <li>Filter the displayed privilege using the PrivilegeId</li>
                </ul>
            </li>
        </ul>
        <p>
            <em>Note: </em>Both of these pages require Internet Explorer 9,10, or 11.
            Recent publically released versions of Google Chrome and Mozilla Firefox also work.
        </p>
    </body>

    </html>
</template>

<style type="text/css">
body {
    font-family: Segoe UI;
    font-size: small;
    background-color: White;
}
</style>