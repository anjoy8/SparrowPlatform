import { createApp } from 'vue'
import App from './App.vue'
import authentication from './auth/authentication'

// Init adal authentication - then create Vue app.
authentication.initialize().then(_ => {
    /* eslint-disable no-new */
    createApp(App).mount('#app')

  });
