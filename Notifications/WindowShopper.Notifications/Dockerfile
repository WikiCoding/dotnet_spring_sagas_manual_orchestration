LABEL authors="tiago.castro"
FROM maven:3.8.5-openjdk-17 AS build
COPY . .
RUN mvn clean package -DskipTests

FROM openjdk:17.0.1-jdk-slim
COPY --from=build /target/Notifications-0.0.1-SNAPSHOT.jar Notifications.jar
EXPOSE 8082
ENTRYPOINT ["java", "-jar", "Notifications.jar"]