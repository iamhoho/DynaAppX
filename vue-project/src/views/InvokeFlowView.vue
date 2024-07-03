<script setup>
import LookUp from '../components/LookUp.vue';
import SelectFlow from '../components/SelectFlow.vue';
import ActionString from '../components/ActionString.vue';
import ActionBool from '../components/ActionBool.vue';
import ActionNumber from '../components/ActionNumber.vue';
import ActionDate from '../components/ActionDate.vue';
import ActionEntityCollection from '../components/ActionEntityCollection.vue';
import ActionEntity from '../components/ActionEntity.vue';
import ActionEntityReference from '../components/ActionEntityReference.vue';
import { ref, watch } from 'vue';
import { AUTO_ALIGNMENT } from 'element-plus/es/components/virtual-list/src/defaults';

var selectedFlow = ref(null);
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
    req.open("POST", hocrm.getCrmUrl() + path, true);
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
    req.open("POST", hocrm.getCrmUrl() + path, true);
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
        <select-flow :required="true" :disabled="false" v-model="selectedFlow"></select-flow>
        <look-up v-if="selectedFlow?.primaryentity && selectedFlow.primaryentity != 'none'"
            :logical-Name="selectedFlow?.primaryentity" :required="true" :disabled="selectedFlow == null"
            v-model="selectedRecord"></look-up>
    </div>
    <div style="display: flex;flex-direction: row; justify-content: center;"><el-button type="success"
            @click="invoke">Invoke</el-button></div>
    <div style="display: flex;flex-direction: row; justify-content: center;">{{ response }}</div>
    <div>
        <action-string v-model="actionString" :required="true" att-name="string" :disabled="false"></action-string>
        <action-bool v-model="actionBool" :required="true" att-name="bool" :disabled="false"></action-bool>
        <action-number v-model="actionNumber" :required="true" att-name="number" :disabled="false"></action-number>
        <action-date v-model="actionDatetime" :required="true" att-name="date" :disabled="false"></action-date>
        <action-entity-collection v-model="actionEntityCollection" :required="true" att-name="entityCollection"
            :disabled="false"></action-entity-collection>
        <action-entity v-model="actionEntity" :required="true" att-name="entity" :disabled="false"></action-entity>
        <action-entity-reference v-model="actionEntityReference" :required="true" att-name="entityReference"
            :disabled="false"></action-entity-reference>
    </div>
</template>
