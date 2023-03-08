FROM nginx:stable-alpine as production-stage
COPY nginx.conf /etc/nginx/conf.d/default.conf

#COPY ./test.js .
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]