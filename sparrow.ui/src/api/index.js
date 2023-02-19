import axios from 'axios';
import router from '../route'
import Vue from 'vue';

import applicationUserManager from "../auth/authentication";

axios.interceptors.push(function (request, next) {
    auth.acquireToken().then(token => {
      // Set default request headers for every request
      request.headers.set('Content-Type', 'application/json');
      request.headers.set('Ocp-Apim-Subscription-Key', 'api key');
      request.headers.set('Authorization', 'Bearer ' + token)
      // continue to next interceptor
      next();
    });
  });