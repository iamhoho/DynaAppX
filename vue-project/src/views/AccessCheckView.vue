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
const userRoles = ref([]);
const accessList = ref(['ReadAccess', 'WriteAccess', 'CreateAccess', 'DeleteAccess', 'ShareAccess', 'AssignAccess', 'AppendAccess', 'AppendToAccess'])

function loadData(userLookUp, recordLookUp) {
    getuserRoles(userLookUp);
    accessCheck(userLookUp, recordLookUp);
}

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

function getuserRoles(userLookUp) {
    if (userLookUp == null) {
        userRoles.value = [];
        return;
    }
    let fetchStr = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'><entity name='role'><attribute name='name' /><link-entity name='systemuserroles' from='roleid' to='roleid' visible='false' intersect='true'><link-entity name='systemuser' from='systemuserid' to='systemuserid' alias='ac'><filter type='and'><condition attribute='systemuserid' operator='eq' value = '" + userLookUp.id + "' /></filter> </link-entity></link-entity></entity></fetch>";
    userRoles.value = daxHelper.fetch("roles", fetchStr, true).value;
}

watch(() => selectedEntity.value, (newValue, oldValue) => {
    selectedRecord.value = null;
})
watch(() => selectedRecord.value, (newValue, oldValue) => {
    loadData(selectedUser.value, selectedRecord.value);

})
watch(() => selectedUser.value, (newValue, oldValue) => {
    loadData(selectedUser.value, selectedRecord.value);

})

</script>

<template>
    <div
        style="display: flex;flex-direction: row;justify-content: center;border-bottom: 1px solid #dddddd;margin-bottom: 20px;">
        <LookUp lableName="User" logicalName="systemuser" :required="true" :disabled="false" v-model="selectedUser">
        </LookUp>
        <EntityControl lableName="Entity" :required="true" ref="selectEntity" :disabled="false" v-model="selectedEntity">
        </EntityControl>
        <LookUp lableName="Record" :logicalName="selectedEntity?.LogicalName" :required="true"
            :disabled="selectedEntity == null" v-model="selectedRecord"></LookUp>
    </div>
    <div style="display: flex;flex-wrap: wrap;flex-direction: row;justify-content: center;">
        <div v-if="selectedUser != null" v-for=" userRole in userRoles">
            <el-tag class="role-tag" type="success">{{ userRole.name }}</el-tag>
        </div>
    </div>

    <div style="display: flex;flex-wrap: wrap;flex-direction: row;justify-content: center; margin-top: 12px;"
        v-if="selectedUser != null && selectedRecord != null">
        <el-button v-for="k in accessList" :key="k" :type="accessRights?.indexOf(k) > -1 ? 'success' : 'danger'">{{ k
        }}</el-button>
    </div>
</template>
<style>
.role-tag {
    margin: 0 8px 8px 0;
}
</style>