LABEL authors="tiago.castro"
FROM maven:3.8.5-openjdk-17 AS build
COPY . .
RUN mvn clean package -DskipTests

FROM openjdk:17.0.1-jdk-slim
COPY --from=build /target/Inventory-Queries-0.0.1-SNAPSHOT.jar Inventory-Queries.jar
EXPOSE 8081
ENTRYPOINT ["java", "-jar", "springtodorestapi.jar"]
