using PaymentService.DataAccess.Postgres.Models;
using PaymentService.WebApi.Contracts;
using PaymentService.WebApi.Controllers;
using PaymentService.WebApi.UseCases.Commands;
using Riok.Mapperly.Abstractions;


namespace PaymentService.WebApi.Mappers
{
    [Mapper]
    public partial class PaymentMapper
    {
        // 1. Маппинг из Payment (БД) в PaymentSucceededV1 (Kafka)
        public partial PaymentSucceededV1 ToPaymentSucceededV1(Payment payment);

        // 2. Маппинг из DTO CreateOrderRequest (из тела запроса) в сущность Order (для БД)
        // Это используется в create.
        public partial Payment ToPaymentEntity(CreatePaymentRequest request);
        public partial Payment ToPaymentEntity(CreatePaymentCommand command);

        //// 3. Маппинг из сущности Order (из БД) в DTO GetOrderResponse (для GET запроса)
        //// Это используется в твоем методе Get() для возврата данных клиенту.
        public partial GetPaymentResponse ToGetPaymentResponse(Payment payment);
    }
}
