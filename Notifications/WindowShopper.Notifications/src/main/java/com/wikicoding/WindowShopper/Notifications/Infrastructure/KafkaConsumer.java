package com.wikicoding.WindowShopper.Notifications.Infrastructure;

import lombok.extern.slf4j.Slf4j;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.kafka.annotation.KafkaListener;
import org.springframework.stereotype.Component;

@Component
@Slf4j
public class KafkaConsumer {
    private final String topic = "order-notifications-topic";
    private final Logger logger = LoggerFactory.getLogger(KafkaConsumer.class);

    @KafkaListener(topics = topic)
    public void listen(String message) {
        logger.warn("Received Message: {}", message);

        String orderStatus = message.split(":")[0];
        String orderId = message.split(":")[1];

        logger.warn("Dispatching email saying: {} to OrderId {}", orderStatus, orderId);
    }
}
