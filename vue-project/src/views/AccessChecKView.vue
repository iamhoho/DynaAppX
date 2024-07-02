<script setup>
import TheWelcome from '../components/TheWelcome.vue'
</script>

<template>
  <main>
    <TheWelcome />
    <div style="display: flex;flex-direction: row;justify-content: center;">
            <select-entity :required="true" ref="selectEntity" :disabled="false"
                v-model="selectedEntity"></select-entity>
            <look-up :logical-Name="selectedEntity?.LogicalName" :required="true" :disabled="selectedEntity == null"
                v-model="selectedRecord"></look-up>
            <look-up logical-Name="systemuser" :required="true" :disabled="false" v-model="selectedUser"></look-up>
        </div>
        <div style="display: flex;flex-direction: row;justify-content: center;"
            v-if="selectedUser!= null && selectedRecord!= null">
            <el-button :type="accessRights?.indexOf('ReadAccess')>-1 ? 'success':'danger'">ReadAccess</el-button>
            <el-button :type="accessRights?.indexOf('WriteAccess')>-1 ? 'success':'danger'">WriteAccess</el-button>
            <el-button :type="accessRights?.indexOf('CreateAccess')>-1 ? 'success':'danger'">CreateAccess</el-button>
            <el-button :type="accessRights?.indexOf('DeleteAccess')>-1 ? 'success':'danger'">DeleteAccess</el-button>
            <el-button :type="accessRights?.indexOf('ShareAccess')>-1 ? 'success':'danger'">ShareAccess</el-button>
            <el-button :type="accessRights?.indexOf('AssignAccess')>-1 ? 'success':'danger'">AssignAccess</el-button>
            <el-button :type="accessRights?.indexOf('AppendAccess')>-1 ? 'success':'danger'">AppendAccess</el-button>
            <el-button
                :type="accessRights?.indexOf('AppendToAccess')>-1 ? 'success':'danger'">AppendToAccess</el-button>
        </div>
  </main>
</template>
