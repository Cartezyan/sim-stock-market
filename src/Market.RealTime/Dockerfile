FROM node:alpine

COPY ./ /app

WORKDIR /app
RUN npm install && npm run build

EXPOSE 1337

ENTRYPOINT ["npm", "start"]
