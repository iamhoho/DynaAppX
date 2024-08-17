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
const userTeams = ref([]);
const accessList = ref(['ReadAccess', 'WriteAccess', 'CreateAccess', 'DeleteAccess', 'ShareAccess', 'AssignAccess', 'AppendAccess', 'AppendToAccess'])

function loadData(userLookUp, recordLookUp) {
    getUserRoles(userLookUp);
    getUserTeams(userLookUp);
    accessCheck(userLookUp, recordLookUp);
}
function accessCheck(userLookUp, recordLookUp) {
    if (userLookUp == null || recordLookUp == null) {
        accessRights.value = null;
        return;
    }
    let path = `systemusers(${userLookUp.id})/Microsoft.Dynamics.CRM.RetrievePrincipalAccess(Target=@tid)?@tid={'@odata.id':'${recordLookUp.entitySetName}(${recordLookUp.id})'}`;
    accessRights.value = daxHelper.retrieve(path, false).AccessRights;
}
function getUserRoles(userLookUp) {
    if (userLookUp == null) {
        userRoles.value = [];
        return;
    }
    let fetchStr = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'><entity name='role'><attribute name='name' />><attribute name='roleid' /><link-entity name='systemuserroles' from='roleid' to='roleid' visible='false' intersect='true'><link-entity name='systemuser' from='systemuserid' to='systemuserid' alias='ac'><filter type='and'><condition attribute='systemuserid' operator='eq' value = '" + userLookUp.id + "' /></filter> </link-entity></link-entity></entity></fetch>";
    userRoles.value = daxHelper.fetch("roles", fetchStr, true).value;
}
function getUserTeams(userLookUp) {
    if (userLookUp == null) {
        userRoles.value = [];
        return;
    }
    let url = "systemusers(" + userLookUp.id + ")/teammembership_association?$select=name,teamid";
    userTeams.value = daxHelper.retrieve(url, true).value;
}
function goToRolePage(roleId) {
    window.open(daxHelper.getCrmUrl() + "/main.aspx?etn=role&id=" + roleId + "&pagetype=entityrecord", "_blank");
}
function goToTeamPage(teamId) {
    window.open(daxHelper.getCrmUrl() + "/main.aspx?etn=team&id=" + teamId + "&pagetype=entityrecord", "_blank");
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
        <LookUp labelName="User" logicalName="systemuser" :required="true" :disabled="false" v-model="selectedUser">
        </LookUp>
        <EntityControl labelName="Entity" :required="true" ref="selectEntity" :disabled="false" v-model="selectedEntity">
        </EntityControl>
        <LookUp labelName="Record" :logicalName="selectedEntity?.LogicalName" :required="true"
            :disabled="selectedEntity == null" v-model="selectedRecord"></LookUp>
    </div>

    <div v-if="selectedUser != null">
        <div style="display: flex;flex-wrap: wrap;flex-direction: row;justify-content: center; margin-top: 12px;">
            <h3>User Roles</h3>
        </div>
        <div style="display: flex;flex-wrap: wrap;flex-direction: row;justify-content: center; margin-top: 12px;">
            <div v-for=" userRole in userRoles">
                <el-button class="btn" type="success" @click="goToRolePage(userRole.roleid)">{{ userRole.name
                }}</el-button>
            </div>
        </div>
        <div style="display: flex;flex-wrap: wrap;flex-direction: row;justify-content: center; margin-top: 12px;">
            <h3>User Teams</h3>
        </div>
        <div v-if="selectedUser != null"
            style="display: flex;flex-wrap: wrap;flex-direction: row;justify-content: center; margin-top: 12px;">
            <div v-for=" userTeam in userTeams">
                <el-button class="btn" type="primary" @click="goToTeamPage(userTeam.teamid)">{{ userTeam.name
                }}</el-button>
            </div>
        </div>
    </div>

    <div v-if="selectedUser != null && selectedRecord != null">
        <div style="display: flex;flex-wrap: wrap;flex-direction: row;justify-content: center; margin-top: 12px;">
            <h3>Record Access Rights</h3>
        </div>
        <div style="display: flex;flex-wrap: wrap;flex-direction: row;justify-content: center; margin-top: 12px;">
            <el-tag size="large" class="tag" v-for="k in accessList" :key="k"
                :type="accessRights?.indexOf(k) > -1 ? 'success' : 'danger'">{{
                    k
                }}</el-tag>
        </div>
    </div>
</template>
<style>
.btn {
    margin: 0 8px 8px 0;
}

.tag {
    margin: 0 8px 8px 0;
}
</style>