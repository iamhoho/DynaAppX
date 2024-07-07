export const daxHelper = {
    getCrmUrl: function () {
        if (import.meta.env.VITE_DEVURL) {
            return import.meta.env.VITE_DEVURL;
        }
        else if (daxHelper.crmUrl) {
            return daxHelper.crmUrl;
        }
        else {
            let url = window.location.href.substring(0, window.location.href.indexOf("/WebResources"));
            if (url.match(/\/$/)) {
                url = url.substring(0, url.length - 1);
            }
            daxHelper.crmUrl = url;
            return daxHelper.crmUrl;
        }
    },
    isGuid: function (str) {
        const guidRegex = /^[{(]?[0-9A-Fa-f]{8}[-]?([0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}[)}]?$/i;
        return guidRegex.test(str);
    },
    fetch: function (entitySetName, fetchXml, useFormattedValue) {
        let fetchStr = `${daxHelper.getCrmUrl()}/api/data/v8.0/${entitySetName}?fetchXml=${fetchXml}`;
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
    getEntityDefinitions: function () {
        if (!daxHelper.entityDefinitions) {
            const xhr = new XMLHttpRequest;
            const path = '/api/data/v8.0/EntityDefinitions?$select=LogicalName,DisplayName,SchemaName,ObjectTypeCode'
            xhr.open("GET", encodeURI(daxHelper.getCrmUrl() + path), false);
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