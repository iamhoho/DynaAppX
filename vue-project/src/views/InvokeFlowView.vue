<script setup>
import LookUpControl from '../components/control/LookUpControl.vue';
import FlowControl from '../components/control/FlowControl.vue';
import StringControl from '../components/control/StringControl.vue';
import BoolControl from '../components/control/BoolControl.vue';
import NumberControl from '../components/control/NumberControl.vue';
import DateControl from '../components/control/DateControl.vue';
import ActionEntityCollectionControl from '../components/control/ActionEntityCollectionControl.vue';
import ActionEntityControl from '../components/control/ActionEntityControl.vue';
import ActionEntityReferenceControl from '../components/control/ActionEntityReferenceControl.vue';
import { daxHelper } from '../daxHelper.js';
import { ref, watch } from 'vue';
import xmljs from 'xml-js';

const selectedFlow = ref(null);
const selectedRecord = ref(null);
const response = ref(null);
const actionInputFields = ref([]);
const actionInputData = ref({});
const invokeHistory = ref([]);
const loading = ref(false);
const isRawdModel = ref(false);
const rawdModelData = ref("");

watch(() => selectedFlow.value, (newValue) => {
    getactionInputFields(newValue);
    actionInputData.value = {};
    selectedRecord.value = null;
})

function getactionInputFields(newValue) {
    actionInputFields.value = [];
    if (!newValue) { return; }
    if (newValue.xaml) {
        JSON.parse(xmljs.xml2json(newValue.xaml)).elements[0].elements.filter((item) => {
            return item.name == 'x:Members';
        })[0].elements.forEach((item) => {
            let name = item.attributes.Name;
            let type = item.attributes.Type;
            if (name != 'InputEntities' && name != 'CreatedEntities' && name != 'Target' && type.indexOf('InArgument') >= 0) {
                let required = item.elements[0].elements.filter((x) => {
                    return x.name == "mxsw:ArgumentRequiredAttribute";
                })[0].attributes.Value
                actionInputFields.value.push({ name: name, type: type, required: daxHelper.stringToBool(required) })
            }
        })
    }
}

function invoke() {
    if (!selectedFlow.value) {
        return;
    }
    try {
        loading.value = true
        if (selectedFlow.value.category == 0) {
            invokeWorkFlow();
        }
        else if (selectedFlow.value.category == 3) {
            invokeAction();
        }
    } catch (error) {
    }
    loading.value = false;
}

function invokeWorkFlow() {
    const path = `v8.0/workflows(${selectedFlow.value.id})/Microsoft.Dynamics.CRM.ExecuteWorkflow`;
    let reqBody = JSON.stringify({ "EntityId": selectedRecord.value.id });
    let req = new XMLHttpRequest();
    req.open("POST", daxHelper.getWebAPIUrl() + path, true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            invokeHistory.value.unshift({ name: selectedFlow.value.name, url: req.responseURL, requestBody: reqBody, statusCode: req.status, response: daxHelper.decodeUnicode(req.responseText), invokeDate: new Date() });
        }
    };
    req.send(reqBody);
}

function invokeAction() {
    let path = "";
    if (selectedFlow.value.primaryentity && selectedFlow.value.primaryentity != "none") {
        path = `${selectedRecord.value.entitySetName}(${selectedRecord.value.id})/Microsoft.Dynamics.CRM.${selectedFlow.value.uniquename}`;
    }
    else {
        path = `${selectedFlow.value.uniquename}`;
    }
    let reqBody = null;
    if (isRawdModel.value) {
        reqBody = rawdModelData.value;
    } else {
        reqBody = JSON.stringify(actionInputData.value);
    }

    let req = new XMLHttpRequest();
    req.open("POST", daxHelper.getWebAPIUrl() + path, true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            invokeHistory.value.unshift({ name: selectedFlow.value.name, url: req.responseURL, requestBody: reqBody, statusCode: req.status, response: daxHelper.decodeUnicode(req.responseText), invokeDate: new Date() });
        }
    };
    req.send(reqBody);
}
</script>

<template>
    <div v-loading="loading">
        <div
            style="display: flex;flex-direction: row; justify-content: center;align-items: center;border-bottom: 1px solid #dddddd;margin-bottom: 20px;">
            <FlowControl lableName=" Flow" :required="true" :disabled="false" v-model="selectedFlow">
            </FlowControl>
            <LookUpControl :lableName="'Record(' + selectedFlow?.primaryentity + ')'"
                v-if="selectedFlow?.primaryentity && selectedFlow.primaryentity != 'none'"
                :logicalName="selectedFlow?.primaryentity" :required="true" :disabled="selectedFlow == null"
                v-model="selectedRecord"></LookUpControl>
            <el-switch v-if="selectedFlow?.category == 3" v-model="isRawdModel" size="large" active-text="Raw Model"
                inactive-text="Field Model" style="--el-switch-on-color: #13ce66; --el-switch-off-color: #13ce66" />
            <el-button style="margin-left: 50px;" type="success" @click="invoke">Invoke</el-button>
        </div>

        <div class="invokeFields" v-if="selectedFlow?.category == 3"
            style="display: flex;flex-direction: row; justify-content: center;align-items: center;border-bottom: 1px solid #dddddd;margin-bottom: 20px;">
            <div>
                <h3 style="display: flex;flex-direction: row;justify-content: center;align-items: center;">Flow
                    parameters
                </h3>
                <div v-if="isRawdModel">
                    <StringControl v-model="rawdModelData" :required="false" lableName="Parameters" :disabled="false">
                    </StringControl>
                </div>
                <div v-else>
                    <div v-for="actionField in actionInputFields" class="invokeForm">
                        <div v-if="actionField.type == 'InArgument(mxs:EntityCollection)'">
                            <ActionEntityCollectionControl v-model="actionInputData[actionField.name]"
                                :required="actionField.required" :lableName="actionField.name" :disabled="false">
                            </ActionEntityCollectionControl>
                        </div>
                        <div v-else-if="actionField.type == 'InArgument(x:Boolean)'">
                            <BoolControl v-model="actionInputData[actionField.name]" :required="actionField.required"
                                :lableName="actionField.name" :disabled="false"></BoolControl>
                        </div>
                        <div
                            v-else-if="actionField.type == 'InArgument(x:Double)' || actionField.type == 'InArgument(mxs:Money)' || actionField.type == 'InArgument(x:Decimal)' || actionField.type == 'InArgument(mxs:OptionSetValue)' || actionField.type == 'InArgument(x:Int32)'">
                            <NumberControl v-model="actionInputData[actionField.name]" :required="actionField.required"
                                :lableName="actionField.name" :disabled="false">
                            </NumberControl>
                        </div>
                        <div v-else-if="actionField.type == 'InArgument(mxs:EntityReference)'">
                            <ActionEntityReferenceControl v-model="actionInputData[actionField.name]"
                                :required="actionField.required" :lableName="actionField.name" :disabled="false">
                            </ActionEntityReferenceControl>
                        </div>
                        <div v-else-if="actionField.type == 'InArgument(s:DateTime)'">
                            <DateControl v-model="actionInputData[actionField.name]" :required="actionField.required"
                                :lableName="actionField.name" :disabled="false"></DateControl>
                        </div>
                        <div v-else-if="actionField.type == 'InArgument(x:String)'">
                            <StringControl v-model="actionInputData[actionField.name]" :required="actionField.required"
                                :lableName="actionField.name" :disabled="false"></StringControl>
                        </div>
                        <div v-else-if="actionField.type == 'InArgument(mxs:Entity)'">
                            <ActionEntityControl v-model="actionInputData[actionField.name]"
                                :required="actionField.required" :lableName="actionField.name" :disabled="false">
                            </ActionEntityControl>
                        </div>
                        <div v-else>
                            <div>Unsupported Field:</div>
                            <div>Field Name:{{ actionField.name }}</div>
                            <div>Field Type:{{ actionField.type }}</div>
                            <div>Required:{{ actionField.required }}</div>
                        </div>
                    </div>
                </div>
            </div>
        </div>


        <div v-if="invokeHistory.length > 0" style="display: flex;flex-direction: row; justify-content: center;">
            <div>
                <h3 style="display: flex;flex-direction: row;justify-content: center;align-items: center;">Invoke
                    History
                </h3>
                <el-table :data="invokeHistory" style="width: 100%" max-height="1000" stripe>
                    <el-table-column fixed prop="invokeDate" label="InvokeDate" width="100" />
                    <el-table-column prop="name" label="Name" width="100" />
                    <el-table-column prop="url" label="URL" width="150" />
                    <el-table-column prop="requestBody" label="RequestBody" width="380" />
                    <el-table-column prop="response" label="Response" width="380" />
                    <el-table-column prop="statusCode" label="StatusCode" width="100" />
                </el-table>
            </div>
        </div>
    </div>
</template>

<style scoped>
.invokeFields:deep(.controlLable) {
    width: 120px;
    word-break: break-all;
}

.invokeFields:deep(.controlItem) {
    width: 250px;
}

.invokeFields:deep(.controlItemTextarea) {
    width: 500px;
}
</style>