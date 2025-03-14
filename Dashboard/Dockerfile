FROM node:20-alpine AS build
WORKDIR /app

# Copy package.json and install dependencies
COPY package*.json ./
RUN npm install

# Copy the rest of the files and build the app
COPY . .
RUN npm run build

# For production environment
FROM node:20-alpine
WORKDIR /app

# Copy built files from the build stage
COPY --from=build /app/dist /app/dist
COPY --from=build /app/package*.json /app/

# Install only production dependencies and serve package
RUN npm install --only=production && \
    npm install -g serve

EXPOSE 80

# Serve the app using serve (a simple static file server)
CMD ["serve", "-s", "dist", "-p", "80"] 