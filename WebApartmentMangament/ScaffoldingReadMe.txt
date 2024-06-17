Scaffolding has generated all the files and added the required dependencies.

However the Application's Startup code may require additional changes for things to work end to end.
Add the following code to the Configure method in your Application's Startup class if not already done:

        app.UseEndpoints(endpoints =>
        {
          endpoints.MapControllerRoute(
            name : "areas",
            pattern : "{area:exists}/{controller=Home}/{action=Index}/{id?}"
          );
        });


đăng nhâp 2 phần 
- admin sẽ phân biệt bằng role xem tài khoản có phải của admin ko 
r mới đăng nhập  nếu tk người dùng đá về user 
- người dùng cho phép tất cả các loại tk đăng nhập 
- store procedue 
