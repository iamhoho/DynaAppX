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
        const guidRegex = /^[{(]?[0-9A-Fa-f]{8}[-]?([0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}[)}]?$/i;
        return guidRegex.test(str);
    },
    fetch: function (entitySetName, fetchXml, useFormattedValue) {
        let fetchStr = `${daxHelper.getWebAPIUrl()}${entitySetName}?fetchXml=${fetchXml}`;
        const xhr = new XMLHttpRequest;
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
        var req = RekTec.ajax.getXHR();
        req.open("GET", encodeURI(daxHelper.getWebAPIUrl() + queryString), false);
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        if (useFormattedValue) {
            req.setRequestHeader("Prefer", "odata.include-annotations=\"OData.Community.Display.V1.FormattedValue\"");
        }
        req.send();
        if (req.status == 200)
            return JSON.parse(req.responseText);
        else
            throw new Error(JSON.parse(req.responseText).error.message);
    },
    getEntityDefinitions: function () {
        if (!daxHelper.entityDefinitions) {
            const xhr = new XMLHttpRequest;
            const path = 'EntityDefinitions?$select=LogicalName,DisplayName,SchemaName,ObjectTypeCode';
            xhr.open("GET", encodeURI(daxHelper.getWebAPIUrl() + path), false);
            xhr.withCredentials = true;
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
    getEntityDefinition: function (metadataId) {
        if (!daxHelper.isGuid(metadataId)) {
            return null;
        }
        const xhr = new XMLHttpRequest;
        const path = 'EntityDefinitions(' + metadataId + ')?$expand=Attributes';
        xhr.open("GET", encodeURI(daxHelper.getWebAPIUrl() + path), false);
        xhr.withCredentials = true;
        xhr.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        xhr.setRequestHeader("OData-MaxVersion", "4.0");
        xhr.setRequestHeader("OData-Version", "4.0");
        xhr.send();
        if (xhr.status === 200) {
            return JSON.parse(xhr.responseText);
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
                                <condition attribute="createdby" operator="eq-userid"/>
                                </filter>
                            </entity>
                            </fetch>`;
            daxHelper.uesrTimezoneBias = daxHelper.fetch("usersettingscollection", fetchStr, true).value[0]['timezonebias'];
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
    }
};