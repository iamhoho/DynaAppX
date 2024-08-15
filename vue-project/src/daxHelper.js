export const daxHelper = {
    getVersion: function () {
        return "8.0";
    },
    getCrmUrl: function () {
        if (!daxHelper.crmUrl) {
            if (import.meta.env.VITE_DEVURL) {
                daxHelper.crmUrl = import.meta.env.VITE_DEVURL;
            }
            else {
                let url = window.location.href.substring(0, window.location.href.indexOf("/WebResources"));
                if (url.match(/\/$/)) {
                    url = url.substring(0, url.length - 1);
                }
                daxHelper.crmUrl = url;
            }
        }
        return daxHelper.crmUrl;

    },
    getWebAPIUrl: function () {
        return daxHelper.getCrmUrl() + "/api/data/v" + daxHelper.getVersion() + "/";
    },
    isGuid: function (str) {
        let guidRegex = /^[{(]?[0-9A-Fa-f]{8}[-]?([0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}[)}]?$/i;
        return guidRegex.test(str);
    },
    fetch: function (entitySetName, fetchXml, useFormattedValue) {
        let fetchStr = `${daxHelper.getWebAPIUrl()}${entitySetName}?fetchXml=${fetchXml}`;
        let xhr = new XMLHttpRequest;
        xhr.open("GET", encodeURI(fetchStr), false);
        xhr.setRequestHeader("Accept", "application/json");
        xhr.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        xhr.setRequestHeader("OData-MaxVersion", "4.0");
        xhr.setRequestHeader("OData-Version", "4.0");
        if (useFormattedValue) {
            xhr.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        }
        xhr.send();
        if (xhr.status == 200) {
            return JSON.parse(xhr.responseText);
        }
        else {
            return [];
        }
    },
    retrieve: function (queryString, useFormattedValue) {
        let xhr = new XMLHttpRequest;
        xhr.open("GET", encodeURI(daxHelper.getWebAPIUrl() + queryString), false);
        xhr.setRequestHeader("Accept", "application/json");
        xhr.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        xhr.setRequestHeader("OData-MaxVersion", "4.0");
        xhr.setRequestHeader("OData-Version", "4.0");
        if (useFormattedValue) {
            xhr.setRequestHeader("Prefer", "odata.include-annotations=\"OData.Community.Display.V1.FormattedValue\"");
        }
        xhr.send();
        if (xhr.status == 200)
            return JSON.parse(xhr.responseText);
        else
            throw new Error(JSON.parse(xhr.responseText).error.message);
    },
    httpGetWebApiJsonResponse: function (path) {
        let xhr = new XMLHttpRequest;
        xhr.open("GET", encodeURI(daxHelper.getWebAPIUrl() + path), false);
        xhr.setRequestHeader("Accept", "application/json");
        xhr.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        xhr.setRequestHeader("OData-MaxVersion", "4.0");
        xhr.setRequestHeader("OData-Version", "4.0");
        xhr.send();
        if (xhr.status == 200)
            return JSON.parse(xhr.responseText);
        else
            throw new Error("status:" + xhr.status + " statusText:" + xhr.statusText);

    },
    getEntityDefinitions: function () {
        if (!daxHelper.entityDefinitions) {
            let xhr = new XMLHttpRequest;
            let path = 'EntityDefinitions?$select=LogicalName,DisplayName,SchemaName,ObjectTypeCode';
            xhr.open("GET", encodeURI(daxHelper.getWebAPIUrl() + path), false);
            xhr.setRequestHeader("Content-Type", "application/json; charset=utf-8");
            xhr.setRequestHeader("OData-MaxVersion", "4.0");
            xhr.setRequestHeader("OData-Version", "4.0");
            xhr.send();
            if (xhr.status === 200) {
                daxHelper.entityDefinitions = JSON.parse(xhr.responseText).value;
            } else {
                daxHelper.entityDefinitions = [];
            }
        }
        return daxHelper.entityDefinitions;
    },
    getAttributeDefinition: function (entityLogicalName, attributeLogicalName) {
        return daxHelper.getEntityDefinitionByLogicalName(entityLogicalName).Attributes.filter((x) => {
            return x.LogicalName == attributeLogicalName;
        })[0];
    },

    getEntityDefinitionByLogicalName: function (logicalName) {
        let metadataId = daxHelper.getEntityDefinitions().filter((item) => {
            return (
                item.LogicalName == logicalName
            )
        })[0].MetadataId;
        metadataId = metadataId.replace("{", "").replace("}", "");
        return daxHelper.getEntityDefinitionByMetadataId(metadataId);
    },

    getEntityDefinitionByMetadataId: function (metadataId) {
        if (!daxHelper.isGuid(metadataId)) {
            return null;
        }
        if (daxHelper.entityDefinitionMap.has(metadataId)) {
            return daxHelper.entityDefinitionMap.get(metadataId);
        }
        let xhr = new XMLHttpRequest;
        let path = 'EntityDefinitions(' + metadataId + ')?$expand=Attributes';
        xhr.open("GET", encodeURI(daxHelper.getWebAPIUrl() + path), false);
        xhr.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        xhr.setRequestHeader("OData-MaxVersion", "4.0");
        xhr.setRequestHeader("OData-Version", "4.0");
        xhr.send();
        if (xhr.status === 200) {
            let jsonResult = JSON.parse(xhr.responseText);
            daxHelper.entityDefinitionMap.set(metadataId, jsonResult);
            return jsonResult;
        } else {
            return null;
        }
    },
    getFlowDefinitions: function () {
        if (!daxHelper.flowDefinitions) {
            let fetchStr = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
                            <entity name="workflow">
                                <order attribute="name" descending="false" />
                                <filter type="and">
                                <condition attribute="statecode" operator="eq" value="1" />
                                <filter type="or">
                                    <condition attribute="category" operator="eq" value="3" />
                                    <filter type="and">
                                    <condition attribute="category" operator="eq" value="0" />
                                    <condition attribute="ondemand" operator="eq" value="1" />
                                    </filter>
                                </filter>
                                <condition attribute="type" operator="eq" value="1" />
                                </filter>
                            </entity>
                            </fetch>`;
            daxHelper.flowDefinitions = daxHelper.fetch("workflows", fetchStr, true);
        }

        return daxHelper.flowDefinitions;
    },
    getUesrTimezoneBias: function () {
        if (!daxHelper.uesrTimezoneBias) {
            let fetchStr = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
                            <entity name="usersettings">
                            <attribute name="timezonebias"/>
                                <filter type="and">
                                <condition attribute="systemuserid" operator="eq-userid"/>
                                </filter>
                            </entity>
                            </fetch>`;
            daxHelper.uesrTimezoneBias = daxHelper.fetch("usersettingscollection", fetchStr, true).value[0]['timezonebias'];
        }
        return daxHelper.uesrTimezoneBias;
    },
    getUesrUILanguageId: function () {
        if (!daxHelper.uesrTimezoneBias) {
            let fetchStr = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
                            <entity name="usersettings">
                            <attribute name="uilanguageid"/>
                                <filter type="and">
                                <condition attribute="systemuserid" operator="eq-userid"/>
                                </filter>
                            </entity>
                            </fetch>`;
            daxHelper.uesrTimezoneBias = daxHelper.fetch("usersettingscollection", fetchStr, true).value[0]['uilanguageid'];
        }
        return daxHelper.uesrTimezoneBias;
    },
    stringToBool: function (str) {
        if (str === "true" || str === "1" || str.toLowerCase() === "true") {
            return true;
        } else if (str === "false" || str === "0" || str.toLowerCase() === "false") {
            return false;
        }
        return null;
    },
    decodeUnicode: function (str) {
        return str.replace(/\\u[\dA-Fa-f]{4}/g,
            function (match) {
                return String.fromCharCode(parseInt(match.replace(/\\u/g, ''), 16));
            }
        );
    },
    getFormattedValueString: function (attributeName) {
        return "_" + attributeName + "_value@OData.Community.Display.V1.FormattedValue";
    },
    getLookUpWebApiAttributeName: function (attributeName) {
        return "_" + attributeName + "_value"
    },
    createDeepCopy: function (obj) {
        return JSON.parse(JSON.stringify(obj));
    },
    getCrmDateTimeString: function (date) {
        return date.toISOString().slice(0, -5) + 'Z';
    },
    getLookUpModel: function (id, name, logicalName) {
        if (!daxHelper.isGuid(id)) {
            return {};
        }
        else {
            return {
                logicalName: logicalName,
                id: id,
                name: name
            };
        }
    },
    entityDefinitionMap: new Map(),
};