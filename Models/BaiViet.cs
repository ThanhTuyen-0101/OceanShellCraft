namespace OceanShellCraft.Models
{
    public class BaiViet
    {
        public int Id { get; set; }

        // Cột nào được phép trống thì thêm dấu ?
        public string? HinhThuc { get; set; }

        // Cột bắt buộc thì không có dấu ?
        public string TieuDe { get; set; }

        public string? NoiDungTomTat { get; set; }
        public string? VideoYoutube { get; set; }

        // Bắt buộc nhập
        public string NoiDung { get; set; }

        // Chú ý: Ảnh nền cũng nên để ? (cho phép null) 
        // Mục đích là để khi bạn Sửa bài viết mà không muốn đổi ảnh mới, form sẽ không báo lỗi đòi nhập ảnh.
        public string? AnhNen { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}