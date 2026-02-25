# 1. Базовый Node образ
FROM node:20-alpine AS build
WORKDIR /app

# 2. Копируем package.json и package-lock.json
COPY package*.json ./

# 3. Устанавливаем зависимости
RUN npm install

# 4. Копируем весь проект
COPY . .

# 5. Строим production билд
RUN npm run build && ls -l /app

# 6. Используем легковесный Nginx для сервирования
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

# 7. Открываем порт
EXPOSE 3000

# 8. Запуск Nginx
CMD ["nginx", "-g", "daemon off;"]