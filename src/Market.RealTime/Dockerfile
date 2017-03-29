FROM node:alpine

WORKDIR /app

# add only package.json so docker uses the cache to build image except when a dependency has changed
ADD ./package.json /app/

# install npm packages (populate node_modules), 
# then clean up temp files to make the intermediate image smaller
RUN npm install && npm cache clean && rm -rf ~/.npm

COPY ./ /app

RUN npm run build

EXPOSE 1337

ENTRYPOINT ["npm", "start"]
