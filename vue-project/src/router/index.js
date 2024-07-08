import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import AccessCheckView from '../views/AccessCheckView1.vue'
import InvokeFlowView from '../views/InvokeFlowView.vue'
import MetadataBrowser from '../views/MetadataBrowser.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView
    },
    {
      path: '/accessChecK',
      name: 'accessChecK',

      component: AccessCheckView
    }
    ,
    {
      path: '/invokeFlow',
      name: 'invokeFlow',
      component: InvokeFlowView
    }
    ,
    {
      path: '/metadataBrowser',
      name: 'metadataBrowser',
      component: MetadataBrowser
    }
  ]
})

export default router
