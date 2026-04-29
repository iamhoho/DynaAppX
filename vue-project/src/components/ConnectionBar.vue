<script setup>
import { ref } from 'vue'
import { daxHelper } from '../daxHelper.js'
import { authService } from '../authService.js'
import { ElInput, ElButton, ElDialog, ElMessage } from 'element-plus'

const dialogVisible = ref(false)
const crmUrl = ref(daxHelper.crmUrl || '')
const isAuthEnabled = ref(!!import.meta.env.VITE_AZURE_CLIENT_ID && import.meta.env.VITE_AZURE_CLIENT_ID !== 'YOUR_AZURE_AD_APP_CLIENT_ID')

function openDialog() {
  dialogVisible.value = true
}

async function login() {
  try {
    await authService.login()
    daxHelper.accessToken = await authService.getAccessToken()
    ElMessage.success('Logged in successfully')
  } catch (error) {
    ElMessage.error('Login failed: ' + error.message)
  }
}

async function logout() {
  try {
    await authService.logout()
    daxHelper.accessToken = null
    ElMessage.success('Logged out')
  } catch (error) {
    ElMessage.error('Logout failed: ' + error.message)
  }
}

function connect() {
  if (!crmUrl.value) {
    ElMessage.warning('Please enter CRM URL')
    return
  }
  let url = crmUrl.value.trim()
  if (!url.startsWith('http')) {
    url = 'https://' + url
  }
  if (url.endsWith('/')) {
    url = url.slice(0, -1)
  }
  daxHelper.setCrmUrl(url)
  dialogVisible.value = false
  ElMessage.success('Connected to: ' + url)
}

defineExpose({ openDialog })
</script>

<template>
  <div class="connection-bar">
    <template v-if="isAuthEnabled">
      <span class="status" :class="{ connected: daxHelper.accessToken }">
        {{ daxHelper.accessToken ? 'Logged In' : 'Not Logged In' }}
      </span>
      <el-button v-if="!daxHelper.accessToken" size="small" @click="login">
        Login with OAuth2
      </el-button>
      <el-button v-else size="small" @click="logout">
        Logout
      </el-button>
    </template>
    <template v-else>
      <span class="status" :class="{ connected: daxHelper.crmUrl }">
        {{ daxHelper.crmUrl ? 'Connected' : 'Not Connected' }}
      </span>
      <el-button size="small" @click="openDialog">
        {{ daxHelper.crmUrl ? 'Change CRM' : 'Connect CRM' }}
      </el-button>
    </template>
  </div>

  <el-dialog v-model="dialogVisible" title="Connect to Dynamics 365" width="400px">
    <div class="connect-form">
      <el-input
        v-model="crmUrl"
        placeholder="https://xxx.crm.dynamics.com"
        @keyup.enter="connect"
      />
      <p class="tip">Enter your Dynamics 365 organization URL</p>
    </div>
    <template #footer>
      <el-button @click="dialogVisible = false">Cancel</el-button>
      <el-button type="primary" @click="connect">Connect</el-button>
    </template>
  </el-dialog>
</template>

<style scoped>
.connection-bar {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 5px 10px;
  background: #f5f5f5;
  border-radius: 4px;
  margin-bottom: 10px;
}
.status {
  font-size: 12px;
  color: #999;
}
.status.connected {
  color: #67c23a;
}
.connect-form {
  padding: 10px 0;
}
.tip {
  font-size: 12px;
  color: #999;
  margin: 5px 0 0 0;
}
</style>