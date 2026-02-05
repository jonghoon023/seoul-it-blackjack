using Seoul.It.Blackjack.Backend.Hubs;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // SignalR 서비스를 등록합니다.
        builder.Services.AddSignalR();

        // Swagger(OpenAPI) 서비스를 등록합니다. 이 설정을 통해 웹 브라우저에서 Swagger UI를 렌더링할 수 있어
        // 학생들이 서버가 정상적으로 실행되고 있음을 확인할 수 있습니다.
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // 개발 환경에서만 상세 오류 페이지 사용
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            // 개발 환경에서는 Swagger 미들웨어를 활성화합니다.
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // 라우팅 사용
        app.UseRouting();

        // 엔드포인트 설정: SignalR Hub 매핑
        app.MapHub<GameHub>("/hub/blackjack");

        // 개발 환경이 아니더라도 Swagger UI를 제공하여 빈 API 명세 페이지를 볼 수 있습니다.
        // 이 코드는 옵션이지만, 학생들이 웹 브라우저에서 페이지를 볼 수 있게 하기 위해 활성화했습니다.
        if (!app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // 웹앱 실행
        Console.WriteLine("Blackjack server is running. Endpoint: /hub/blackjack");
        app.Run();
    }
}