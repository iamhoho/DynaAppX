<script setup>
import SelectEntity from '../components/SelectEntity.vue';
import LookUp from '../components/LookUp.vue';
import { hocrm } from '../CRMHelper.js'
import { ref, watch } from 'vue'
import { ElMessage } from 'element-plus'

var selectedEntity = ref(null);
var selectedRecord = ref(null);
var selectedUser = ref(null);
var accessRights = ref(null);

function accessCheck(userLookUp, recordLookUp) {
    if (userLookUp == null || recordLookUp == null) {
        accessRights.value = null;
        return;
    }
    const xhr = new XMLHttpRequest;
    const path = `/api/data/v8.0/systemusers(${userLookUp.id})/Microsoft.Dynamics.CRM.RetrievePrincipalAccess(Target=@tid)?@tid={'@odata.id':'${recordLookUp.entitySetName}(${recordLookUp.id})'}`;
    xhr.open("GET", encodeURI(hocrm.getCrmUrl() + path), false);
    xhr.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    xhr.setRequestHeader("OData-MaxVersion", "4.0");
    xhr.setRequestHeader("OData-Version", "4.0");
    xhr.send();
    if (xhr.status == 200) {
        accessRights.value = JSON.parse(xhr.responseText).AccessRights;
    }
    else {
        ElMessage.error(`${xhr.responseURL} ${xhr.status} ${xhr.statusText}`);
    }
}
watch(selectedEntity, (newValue, oldValue) => {
    selectedRecord.value = null;
})
watch(selectedRecord, (newValue, oldValue) => {
    accessCheck(selectedUser.value, selectedRecord.value);

})
watch(selectedUser, (newValue, oldValue) => {
    accessCheck(selectedUser.value, selectedRecord.value);

})
</script>

<template>
    <div style="display: flex;flex-direction: row;justify-content: center;">
        <select-entity :required="true" ref="selectEntity" :disabled="false" v-model="selectedEntity"></select-entity>
        <look-up :logicalName="selectedEntity?.LogicalName" :required="true" :disabled="selectedEntity == null"
            v-model="selectedRecord"></look-up>
        <look-up logicalName="systemuser" :required="true" :disabled="false" v-model="selectedUser"></look-up>
    </div>
    <div style="display: flex;flex-direction: row;justify-content: center;"
        v-if="selectedUser != null && selectedRecord != null">
        <el-button :type="accessRights?.indexOf('ReadAccess') > -1 ? 'success' : 'danger'">ReadAccess</el-button>
        <el-button :type="accessRights?.indexOf('WriteAccess') > -1 ? 'success' : 'danger'">WriteAccess</el-button>
        <el-button :type="accessRights?.indexOf('CreateAccess') > -1 ? 'success' : 'danger'">CreateAccess</el-button>
        <el-button :type="accessRights?.indexOf('DeleteAccess') > -1 ? 'success' : 'danger'">DeleteAccess</el-button>
        <el-button :type="accessRights?.indexOf('ShareAccess') > -1 ? 'success' : 'danger'">ShareAccess</el-button>
        <el-button :type="accessRights?.indexOf('AssignAccess') > -1 ? 'success' : 'danger'">AssignAccess</el-button>
        <el-button :type="accessRights?.indexOf('AppendAccess') > -1 ? 'success' : 'danger'">AppendAccess</el-button>
        <el-button :type="accessRights?.indexOf('AppendToAccess') > -1 ? 'success' : 'danger'">AppendToAccess</el-button>
    </div>
</template>
