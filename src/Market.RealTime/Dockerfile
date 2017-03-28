FROM node:alpine

WORKDIR /app

# add only package.json so docker uses the cache to build image except when a dependency has changed
ADD package.json /app

RUN npm install

COPY ./ /app

RUN npm run build

EXPOSE 1337

ENTRYPOINT ["npm", "start"]
