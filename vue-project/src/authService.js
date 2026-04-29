import { PublicClientApplication } from '@azure/msal-browser'

const msalConfig = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_CLIENT_ID || 'YOUR_AZURE_AD_APP_CLIENT_ID',
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AZURE_TENANT_ID || 'common'}`,
    redirectUri: import.meta.env.DEV ? 'http://localhost:5173' : window.location.origin,
    postLogoutRedirectUri: import.meta.env.DEV ? 'http://localhost:5173' : window.location.origin,
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: false,
  },
}

export const msalInstance = new PublicClientApplication(msalConfig)

export const authService = {
  account: null,

  async initialize() {
    try {
      const response = await msalInstance.handleRedirectPromise()
      if (response) {
        msalInstance.setActiveAccount(response.account)
        this.account = response.account
      } else {
        const accounts = msalInstance.getAllAccounts()
        if (accounts.length > 0) {
          msalInstance.setActiveAccount(accounts[0])
          this.account = accounts[0]
        }
      }
    } catch (error) {
      console.error('MSAL redirect error:', error)
    }
  },

  async login() {
    try {
      const response = await msalInstance.loginPopup({
        scopes: [import.meta.env.VITE_CRM_SCOPES || 'https://*.dynamics.com/.default'],
      })
      this.account = response.account
      return response
    } catch (error) {
      console.error('Login error:', error)
      throw error
    }
  },

  async logout() {
    try {
      await msalInstance.logoutPopup()
      this.account = null
    } catch (error) {
      console.error('Logout error:', error)
      throw error
    }
  },

  async getAccessToken() {
    if (!this.account) {
      throw new Error('No active account. Please login first.')
    }

    try {
      const request = {
        scopes: [import.meta.env.VITE_CRM_SCOPES || 'https://*.dynamics.com/.default'],
        account: this.account,
      }
      const response = await msalInstance.acquireTokenSilent(request)
      return response.accessToken
    } catch (error) {
      console.error('Token acquisition error, trying popup:', error)
      try {
        const response = await msalInstance.acquireTokenPopup({
          scopes: [import.meta.env.VITE_CRM_SCOPES || 'https://*.dynamics.com/.default'],
        })
        return response.accessToken
      } catch (popupError) {
        console.error('Popup token error:', popupError)
        throw popupError
      }
    }
  },

  isAuthenticated() {
    return this.account !== null
  },

  getAccount() {
    return this.account
  },
}

authService.initialize()
