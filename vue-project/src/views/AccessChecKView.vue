<script setup>
import EntityControl from '../components/control/EntityControl.vue';
import LookUp from '../components/control/LookUpControl.vue';
import { daxHelper } from '../daxHelper.js'
import { ref, watch } from 'vue'
import { ElMessage } from 'element-plus'

const selectedEntity = ref(null);
const selectedRecord = ref(null);
const selectedUser = ref(null);
const accessRights = ref(null);

function accessCheck(userLookUp, recordLookUp) {
    if (userLookUp == null || recordLookUp == null) {
        accessRights.value = null;
        return;
    }
    const xhr = new XMLHttpRequest;
    const path = `/api/data/v8.0/systemusers(${userLookUp.id})/Microsoft.Dynamics.CRM.RetrievePrincipalAccess(Target=@tid)?@tid={'@odata.id':'${recordLookUp.entitySetName}(${recordLookUp.id})'}`;
    xhr.open("GET", encodeURI(daxHelper.getCrmUrl() + path), false);
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
watch(() => selectedEntity.value, (newValue, oldValue) => {
    selectedRecord.value = null;
})
watch(() => selectedRecord.value, (newValue, oldValue) => {
    accessCheck(selectedUser.value, selectedRecord.value);

})
watch(() => selectedUser.value, (newValue, oldValue) => {
    accessCheck(selectedUser.value, selectedRecord.value);

})
</script>

<template>
    <div style="display: flex;flex-direction: row;justify-content: center;">
        <EntityControl :required="true" ref="selectEntity" :disabled="false" v-model="selectedEntity">
        </EntityControl>
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
        <el-button
            :type="accessRights?.indexOf('AppendToAccess') > -1 ? 'success' : 'danger'">AppendToAccess</el-button>
    </div>
</template>
