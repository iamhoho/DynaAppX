<script setup>
import LookUpControl from '../components/control/LookUpControl.vue';
import FlowControl from '../components/control/FlowControl.vue';
import StringControl from '../components/control/StringControl.vue';
import BoolControl from '../components/control/BoolControl.vue';
import NumberControl from '../components/control/NumberControl.vue';
import DateControl from '../components/control/DateControl.vue';
import EntityCollectionControl from '../components/control/EntityCollectionControl.vue';
import ActionEntityControl from '../components/control/ActionEntityControl.vue';
import EntityReferenceControl from '../components/control/EntityReferenceControl.vue';
import { ref, watch } from 'vue';
import { AUTO_ALIGNMENT } from 'element-plus/es/components/virtual-list/src/defaults';

const selectedFlow = ref(null);
const selectedRecord = ref(null);
const response = ref(null);
const actionString = ref('123');
const actionBool = ref(true);
const actionNumber = ref(123.456);
const actionDatetime = ref(new Date());
const actionEntityCollection = ref('[]');
const actionEntity = ref('{}');
const actionEntityReference = ref('{}');

watch(() => selectedFlow.value, (newValue) => {
    selectedRecord.value = null;
})

function invoke() {
    if (selectedFlow.value.category == 0) {
        invokeWorkFlow();
    }
    else if (selectedFlow.value.category == 3) {
        invokeAction();
    }

}
function invokeWorkFlow() {
    let that = this;
    const path = `/api/data/v9.0/workflows(${selectedFlow.value.id})/Microsoft.Dynamics.CRM.ExecuteWorkflow`;
    let req = new XMLHttpRequest();
    req.open("POST", daxHelper.getCrmUrl() + path, true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            that.response = JSON.stringify({
                status: req.status,
                responseText: req.responseText
            });
        }
    };
    let body = {
        "EntityId": selectedRecord.value.id
    };
    req.send(JSON.stringify(body));
}
function invokeAction() {
    let that = this;
    let path = "";
    if (selectedFlow.value.primaryentity && selectedFlow.value.primaryentity != "none") {
        path = `/api/data/v9.0/${selectedRecord.value.entitySetName}(${selectedRecord.value.id})/Microsoft.Dynamics.CRM.${selectedFlow.value.uniquename}`;
    }
    else {
        path = `/api/data/v9.0/${selectedFlow.value.uniquename}`;
    }
    let req = new XMLHttpRequest();
    req.open("POST", daxHelper.getCrmUrl() + path, true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            that.response = JSON.stringify({
                status: req.status,
                responseText: req.responseText
            });
        }
    };
    let body = {
        String: "hoho",
        Bool: true,
        Float: 1.23456,
        Dateime: "2021-10-01T14:30:00.0000000+08:00",
        Decimal: 7.89101,
        Int: 777,
        Options: 666,
        Money: 13,
        EntityCollection:
            [
                {
                    "@odata.type": "Microsoft.Dynamics.CRM.account",
                    "accountid": "C4CA0B66-59B9-E611-8106-C4346BDC0E01",
                    "name": "Test",
                    "accountnumber": "123"
                },
                {
                    "@odata.type": "Microsoft.Dynamics.CRM.account",
                    "accountid": "CD67D78E-16BB-E611-8109-C4346BDC3C21",
                    "name": "Test2",
                    "accountnumber": "1234"
                }
            ],
        Entity: {
            "@odata.type": "Microsoft.Dynamics.CRM.account",
            "accountid": "CD67D78E-16BB-E611-8109-C4346BDC3C22",
            "name": "Test3",
            "accountnumber": "1234"
        },
        EntityReference: {
            "@odata.type": "Microsoft.Dynamics.CRM.account",
            "accountid": "CD67D78E-16BB-E611-8109-C4346BDC3C23"
        },
    };
    req.send(JSON.stringify(body));
}
</script>

<template>
    <div style="display: flex;flex-direction: row; justify-content: center;">
        <FlowControl :required="true" :disabled="false" v-model="selectedFlow"></FlowControl>
        <LookUpControl v-if="selectedFlow?.primaryentity && selectedFlow.primaryentity != 'none'"
            :logical-Name="selectedFlow?.primaryentity" :required="true" :disabled="selectedFlow == null"
            v-model="selectedRecord"></LookUpControl>
    </div>
    <div style="display: flex;flex-direction: row; justify-content: center;"><el-button type="success"
            @click="invoke">Invoke</el-button></div>
    <div style="display: flex;flex-direction: row; justify-content: center;">{{ response }}</div>
    <div>
        <StringControl v-model="actionString" :required="true" att-name="string" :disabled="false"></StringControl>
        <BoolControl v-model="actionBool" :required="true" att-name="bool" :disabled="false"></BoolControl>
        <NumberControl v-model="actionNumber" :required="true" att-name="number" :disabled="false">
        </NumberControl>
        <DateControl v-model="actionDatetime" :required="true" att-name="date" :disabled="false"></DateControl>
        <EntityCollectionControl v-model="actionEntityCollection" :required="true" att-name="entityCollection"
            :disabled="false"></EntityCollectionControl>
        <ActionEntityControl v-model="actionEntity" :required="true" att-name="entity" :disabled="false">
        </ActionEntityControl>
        <EntityReferenceControl v-model="actionEntityReference" :required="true" att-name="entityReference"
            :disabled="false"></EntityReferenceControl>
    </div>
</template>
