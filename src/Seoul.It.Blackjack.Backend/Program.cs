using Seoul.It.Blackjack.Backend.Hubs;

var builder = WebApplication.CreateBuilder(args);

// SignalR 서비스를 등록합니다.
builder.Services.AddSignalR();

var app = builder.Build();

// 개발 환경에서만 상세 오류 페이지 사용
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// 라우팅 사용
app.UseRouting();

// 엔드포인트 설정: SignalR Hub 매핑
app.MapHub<GameHub>("/hub/blackjack");

// 웹앱 실행
Console.WriteLine("Blackjack server is running. Endpoint: /hub/blackjack");
app.Run();
