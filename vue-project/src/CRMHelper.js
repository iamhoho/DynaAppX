export const hocrm = {
    getCrmUrl: function () {
        if (import.meta.env.VITE_DEVURL) {
            return import.meta.env.VITE_DEVURL;
        }
        else {
            let url = window.location.href.substring(0, window.location.href.indexOf("/WebResources"));
            if (url.match(/\/$/)) {
                url = url.substring(0, url.length - 1);
            }
            return url;
        }

    },
    isGuid: function (str) {
        const guidRegex = /^[{(]?[0-9A-Fa-f]{8}[-]?([0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}[)}]?$/i;
        return guidRegex.test(str);
    },
    fetch: function (entitySetName, fetchXml, useFormattedValue) {
        let fetchStr = `${hocrm.getCrmUrl()}/api/data/v8.0/${entitySetName}?fetchXml=${fetchXml}`;
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
        if (!hocrm.entityDefinitions) {
            const xhr = new XMLHttpRequest;
            const path = '/api/data/v8.0/EntityDefinitions?$select=LogicalName,DisplayName,SchemaName,ObjectTypeCode'
            xhr.open("GET", encodeURI(hocrm.getCrmUrl() + path), false);
            xhr.setRequestHeader("Content-Type", "application/json; charset=utf-8");
            xhr.setRequestHeader("OData-MaxVersion", "4.0");
            xhr.setRequestHeader("OData-Version", "4.0");
            xhr.send();
            if (xhr.status === 200) {
                hocrm.entityDefinitions = JSON.parse(xhr.responseText).value;
            } else {
                hocrm.entityDefinitions = [];
            }
        }

        return hocrm.entityDefinitions;
    },
    getFlowDefinitions: function () {
        if (!hocrm.flowDefinitions) {
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
            hocrm.flowDefinitions = hocrm.fetch("workflows", fetchStr, true);
        }

        return hocrm.flowDefinitions;
    },
    getUesrTimezoneBias: function () {
        if (!hocrm.uesrTimezoneBias) {
            let fetchStr = `<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
                            <entity name="usersettings">
                            <attribute name="timezonebias"/>
                                <filter type="and">
                                <condition attribute="createdby" operator="eq-userid"/>
                                </filter>
                            </entity>
                            </fetch>`;
            hocrm.uesrTimezoneBias = hocrm.fetch("usersettingscollection", fetchStr, true).value[0]['timezonebias'];
        }
        return hocrm.uesrTimezoneBias;
    }
};