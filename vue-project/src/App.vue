<script setup>
import { RouterLink, RouterView } from 'vue-router'
import DynaAppX from './components/DynaAppX.vue'
import ConnectionBar from './components/ConnectionBar.vue'
import { ref } from 'vue'

const connectionBar = ref(null)
</script>

<template>
  <header>
    <div class="wrapper">
      <DynaAppX />
      <ConnectionBar ref="connectionBar" />
      <nav>
        <RouterLink to="/accessChecK">AccessCheck</RouterLink>
        <RouterLink to="/invokeFlow">InvokeFlow</RouterLink>
        <RouterLink to="/godPage">GodPage</RouterLink>
        <RouterLink to="/metadataBrowser">MetadataBrowser(Powered by Microsoft)</RouterLink>
      </nav>
    </div>
  </header>

  <RouterView />
</template>

<style>
.controlLabel {
  margin-right: 5px;
  font-size: 14px;
}
</style>
<style scoped>
header {
  line-height: 1.5;
  max-height: 100vh;
}

.logo {
  display: block;
  margin: 0 auto 2rem;
}

nav {
  width: 100%;
  font-size: 1.2rem;
  text-align: center;
  margin-top: 1rem;
  margin-bottom: 1rem;
}


nav a {
  display: inline-block;
  padding: 0 1rem;
  border-left: 1px solid var(--color-border);
}

nav a:first-of-type {
  border: 0;
}

nav a.router-link-exact-active {
  color: var(--color-text);
  font-weight: 500;
  border-bottom: 2px solid #000000;
}

nav a.router-link-exact-active:hover {
  background-color: transparent;
}
</style>