using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQWebExample.Watermark.Services;
using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQWebExample.Watermark.BackgroundServices
{
    public class ImageWatermarkProcessBackgroundService : BackgroundService
    {
        private readonly RabbitMQClientService _rabbitMQClientService;
        private readonly ILogger<ImageWatermarkProcessBackgroundService> _logger;
        private IModel _channel;

        public ImageWatermarkProcessBackgroundService(ILogger<ImageWatermarkProcessBackgroundService> logger, RabbitMQClientService rabbitMQClientService)
        {
            _logger = logger;
            _rabbitMQClientService = rabbitMQClientService;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMQClientService.Connect();
            _channel.BasicQos(0, 1, false);
            
            return base.StartAsync(cancellationToken);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            _channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);

            consumer.Received+= Consumer_Received;

            return Task.CompletedTask;
        }
        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            Task.Delay(10000).Wait();
            try
            {
                
            var productImageCreatedEvent= JsonSerializer.Deserialize<ProductImageCreatedEvent>(@event.Body.ToArray());
            
            var path=Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", productImageCreatedEvent.ImageName);

            using var img=Image.FromFile(path);
            using var graphics=Graphics.FromImage(img);
            using var font=new Font(FontFamily.GenericMonospace, 40, FontStyle.Bold, GraphicsUnit.Pixel);

            var textSize=graphics.MeasureString("www.tunahanozcan.com", font);
            var color=Color.FromArgb(128, 255, 255, 255);
            var brush=new SolidBrush(color);
            var position=new Point(img.Width-(int)textSize.Width-10, img.Height-(int)textSize.Height-10);

            graphics.DrawString("www.tunahanozcan.com", font, brush, position);

            img.Save("wwwroot/Images/watermarks/"+productImageCreatedEvent.ImageName);

            img.Dispose();
            graphics.Dispose();
            _channel.BasicAck(@event.DeliveryTag,false);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.Message);
            }
            
            return Task.CompletedTask;
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
        
    }
}
