#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Comic Rental Database Seeder
เพิ่มข้อมูล mock สำหรับ Comic Rental System
"""

import sqlite3
import random
from datetime import datetime, timedelta
import bcrypt

def hash_password(password):
    """สร้าง password hash"""
    salt = bcrypt.gensalt()
    return bcrypt.hashpw(password.encode('utf-8'), salt).decode('utf-8')

def connect_db():
    """เชื่อมต่อ database"""
    return sqlite3.connect('/mnt/f/repos/rent-app/ComicRental/comic_rental.db')

def seed_customers(conn):
    """เพิ่มข้อมูล Customers"""
    customers = [
        ('สมชาย ใจดี', '0891234567', '123 ถนนสุขุมวิท เขตคลองเตย กรุงเทพฯ 10110', '1234567890123', 'somchai@email.com', 'Active'),
        ('สมหญิง รักอ่าน', '0892345678', '456 ถนนพระราม 4 เขตปทุมวัน กรุงเทพฯ 10330', '2345678901234', 'somying@email.com', 'Active'),
        ('นายพีระพล สุขใส', '0893456789', '789 ถนนวิภาวดี เขตจตุจักร กรุงเทพฯ 10900', '3456789012345', 'peerapol@email.com', 'Active'),
        ('นางสาวมาลี ใจงาม', '0894567890', '321 ถนนลาดพร้าว เขตวังทองหลาง กรุงเทพฯ 10310', '4567890123456', 'malee@email.com', 'Active'),
        ('นายสมศักดิ์ การดี', '0895678901', '654 ถนนพหลโยธิน เขตจตุจักร กรุงเทพฯ 10900', '5678901234567', 'somsak@email.com', 'Active'),
        ('นางสาวศิริ วงษ์ใหญ่', '0896789012', '987 ถนนรามคำแหง เขตบางกะปิ กรุงเทพฯ 10240', '6789012345678', 'siri@email.com', 'Active'),
        ('นายธนาคาร เงินล้าน', '0897890123', '147 ถนนสีลม เขตบางรัก กรุงเทพฯ 10500', '7890123456789', 'thanakarn@email.com', 'Active'),
        ('นางสาวปิยะดา ใจบุญ', '0898901234', '258 ถนนเพชรบุรี เขตราชเทวี กรุงเทพฯ 10400', '8901234567890', 'piyada@email.com', 'Active'),
        ('นายอนุชา มีสุข', '0899012345', '369 ถนนอโศก เขตวัฒนา กรุงเทพฯ 10110', '9012345678901', 'anucha@email.com', 'Inactive'),
        ('นางสาวจันทร์เพ็ญ แสงดาว', '0880123456', '741 ถนนบางนา เขตบางนา กรุงเทพฯ 10260', '0123456789012', 'janpen@email.com', 'Active'),
        ('นายกิตติ เก่งการ', '0881234567', '852 ซอยทองหล่อ เขตวัฒนา กรุงเทพฯ 10110', '1234567890124', 'kitti@email.com', 'Active'),
        ('นางสาวนิภา สีใส', '0882345678', '963 ถนนอุดมสุข เขตบางนา กรุงเทพฯ 10260', '2345678901235', 'nipha@email.com', 'Active'),
        ('นายวิชัย ชัยชนะ', '0883456789', '159 ถนนราชดำริ เขตปทุมวัน กรุงเทพฯ 10330', '3456789012346', 'wichai@email.com', 'Active'),
        ('นางสาวรัตนา ทองคำ', '0884567890', '357 ถนนประดิพัทธ์ เขตสามเสน กรุงเทพฯ 10300', '4567890123457', 'rattana@email.com', 'Active'),
        ('นายสุรศักดิ์ ใจกล้า', '0885678901', '456 ถนนจันทน์ เขตทุ่งครุ กรุงเทพฯ 10140', '5678901234568', 'surasak@email.com', 'Active')
    ]
    
    cursor = conn.cursor()
    base_date = datetime(2024, 1, 1)
    
    for i, (name, phone, address, id_card, email, status) in enumerate(customers, 1):
        created_date = (base_date + timedelta(days=random.randint(0, 100))).strftime('%Y-%m-%d %H:%M:%S')
        total_borrowed = random.randint(0, 20)
        total_fines = random.uniform(0, 500)
        
        cursor.execute('''
            INSERT OR REPLACE INTO Customers 
            (CustomerId, FullName, Phone, Address, IdCard, Email, Status, CreatedDate, TotalBorrowed, TotalFines)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        ''', (i, name, phone, address, id_card, email, status, created_date, total_borrowed, total_fines))
    
    print(f"เพิ่มข้อมูล Customers จำนวน {len(customers)} รายการ")

def seed_books(conn):
    """เพิ่มข้อมูล Books"""
    books = [
        # Action Comics
        ('One Piece เล่ม 1', 'Eiichiro Oda', 1, '9784088720012', 'Shueisha', 1, 'A1', 10.00, 'Available', 'Good'),
        ('One Piece เล่ม 2', 'Eiichiro Oda', 1, '9784088720029', 'Shueisha', 2, 'A1', 10.00, 'Available', 'Good'),
        ('One Piece เล่ม 3', 'Eiichiro Oda', 1, '9784088720036', 'Shueisha', 3, 'A1', 10.00, 'Rented', 'Good'),
        ('Naruto เล่ม 1', 'Masashi Kishimoto', 1, '9784088730010', 'Shueisha', 1, 'A2', 10.00, 'Available', 'Good'),
        ('Naruto เล่ม 2', 'Masashi Kishimoto', 1, '9784088730027', 'Shueisha', 2, 'A2', 10.00, 'Damaged', 'Poor'),
        ('Attack on Titan เล่ม 1', 'Hajime Isayama', 1, '9784063842067', 'Kodansha', 1, 'A3', 15.00, 'Available', 'Excellent'),
        ('Dragon Ball เล่ม 1', 'Akira Toriyama', 1, '9784088518018', 'Shueisha', 1, 'A4', 10.00, 'Available', 'Good'),
        ('Demon Slayer เล่ม 1', 'Koyoharu Gotouge', 1, '9784088807317', 'Shueisha', 1, 'A5', 15.00, 'Rented', 'Good'),
        ('My Hero Academia เล่ม 1', 'Kohei Horikoshi', 1, '9784088801926', 'Shueisha', 1, 'A6', 12.00, 'Available', 'Good'),
        ('Jujutsu Kaisen เล่ม 1', 'Gege Akutami', 1, '9784088815947', 'Shueisha', 1, 'A7', 15.00, 'Available', 'Good'),
        
        # Romance Comics
        ('Kimi ni Todoke เล่ม 1', 'Karuho Shiina', 2, '9784088463524', 'Shueisha', 1, 'B1', 10.00, 'Available', 'Good'),
        ('Fruits Basket เล่ม 1', 'Natsuki Takaya', 2, '9784592176718', 'Hakusensha', 1, 'B2', 12.00, 'Available', 'Good'),
        ('Nana เล่ม 1', 'Ai Yazawa', 2, '9784088565026', 'Shueisha', 1, 'B3', 12.00, 'Available', 'Fair'),
        ('Lovely Complex เล่ม 1', 'Aya Nakahara', 2, '9784088463357', 'Shueisha', 1, 'B4', 10.00, 'Rented', 'Good'),
        ('Skip Beat เล่ม 1', 'Yoshiki Nakamura', 2, '9784592176725', 'Hakusensha', 1, 'B5', 12.00, 'Available', 'Good'),
        
        # Comedy Comics
        ('Crayon Shin-chan เล่ม 1', 'Yoshito Usui', 3, '9784575939016', 'Futabasha', 1, 'C1', 8.00, 'Available', 'Good'),
        ('Detective Conan เล่ม 1', 'Gosho Aoyama', 3, '9784091233011', 'Shogakukan', 1, 'C2', 10.00, 'Available', 'Good'),
        ('Gintama เล่ม 1', 'Hideaki Sorachi', 3, '9784088736990', 'Shueisha', 1, 'C3', 10.00, 'Available', 'Good'),
        ('Dr. Stone เล่ม 1', 'Riichiro Inagaki', 3, '9784088811765', 'Shueisha', 1, 'C4', 12.00, 'Rented', 'Good'),
        
        # Drama Comics
        ('Monster เล่ม 1', 'Naoki Urasawa', 4, '9784091860118', 'Shogakukan', 1, 'D1', 18.00, 'Available', 'Excellent'),
        ('Death Note เล่ม 1', 'Tsugumi Ohba', 4, '9784088736781', 'Shueisha', 1, 'D2', 15.00, 'Available', 'Good'),
        ('Tokyo Ghoul เล่ม 1', 'Sui Ishida', 4, '9784088702162', 'Shueisha', 1, 'D3', 15.00, 'Available', 'Good'),
        
        # Fantasy Comics
        ('Fairy Tail เล่ม 1', 'Hiro Mashima', 5, '9784063649031', 'Kodansha', 1, 'E1', 12.00, 'Available', 'Good'),
        ('Seven Deadly Sins เล่ม 1', 'Nakaba Suzuki', 5, '9784063849639', 'Kodansha', 1, 'E2', 12.00, 'Available', 'Excellent'),
        ('Fullmetal Alchemist เล่ม 1', 'Hiromu Arakawa', 5, '9784757514997', 'Square Enix', 1, 'E3', 15.00, 'Rented', 'Good'),
        ('Made in Abyss เล่ม 1', 'Akihito Tsukushi', 5, '9784862767943', 'Takeshobo', 1, 'E4', 18.00, 'Available', 'Good'),
        
        # Horror Comics
        ('Another เล่ม 1', 'Yukito Ayatsuji', 6, '9784048545556', 'Kadokawa', 1, 'F1', 15.00, 'Available', 'Good'),
        ('Junji Ito Collection', 'Junji Ito', 6, '9784091033680', 'Shogakukan', 1, 'F2', 20.00, 'Available', 'Good'),
        ('Parasyte เล่ม 1', 'Hitoshi Iwaaki', 6, '9784063145892', 'Kodansha', 1, 'F3', 15.00, 'Available', 'Good'),
        
        # Sports Comics
        ('Slam Dunk เล่ม 1', 'Takehiko Inoue', 7, '9784088710013', 'Shueisha', 1, 'G1', 10.00, 'Available', 'Good'),
        ('Slam Dunk เล่ม 2', 'Takehiko Inoue', 7, '9784088710020', 'Shueisha', 2, 'G1', 10.00, 'Available', 'Good'),
        ('Captain Tsubasa เล่ม 1', 'Yoichi Takahashi', 7, '9784088518025', 'Shueisha', 1, 'G2', 10.00, 'Rented', 'Good'),
        ('Haikyuu เล่ม 1', 'Haruichi Furudate', 7, '9784088700564', 'Shueisha', 1, 'G3', 12.00, 'Available', 'Good'),
        ('Kuroko no Basket เล่ม 1', 'Tadatoshi Fujimaki', 7, '9784088700083', 'Shueisha', 1, 'G4', 12.00, 'Available', 'Good'),
        
        # Sci-Fi Comics
        ('Ghost in the Shell เล่ม 1', 'Masamune Shirow', 8, '9784063144596', 'Kodansha', 1, 'H1', 18.00, 'Available', 'Excellent'),
        ('Akira เล่ม 1', 'Katsuhiro Otomo', 8, '9784063144589', 'Kodansha', 1, 'H2', 20.00, 'Available', 'Good'),
        ('Steins;Gate เล่ม 1', '5pb.', 8, '9784047155800', 'Kadokawa', 1, 'H3', 15.00, 'Available', 'Good'),
        ('Evangelion เล่ม 1', 'Yoshiyuki Sadamoto', 8, '9784063144602', 'Kodansha', 1, 'H4', 18.00, 'Rented', 'Good')
    ]
    
    cursor = conn.cursor()
    base_date = datetime(2024, 1, 1)
    
    for i, (title, author, category_id, isbn, publisher, volume, shelf, price, status, condition) in enumerate(books, 1):
        created_date = (base_date + timedelta(days=random.randint(0, 30))).strftime('%Y-%m-%d %H:%M:%S')
        qr_code = f"QR{i:04d}"
        
        cursor.execute('''
            INSERT OR REPLACE INTO Books 
            (BookId, Title, Author, CategoryId, Isbn, Publisher, Volume, ShelfLocation, 
             RentalPrice, Status, Condition, QrCode, CreatedDate)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        ''', (i, title, author, category_id, isbn, publisher, volume, shelf, 
              price, status, condition, qr_code, created_date))
    
    print(f"เพิ่มข้อมูล Books จำนวน {len(books)} รายการ")

def seed_rentals(conn):
    """เพิ่มข้อมูล Rentals"""
    cursor = conn.cursor()
    
    # ดึงข้อมูล books ที่มีสถานะ 'Rented'
    cursor.execute("SELECT BookId FROM Books WHERE Status = 'Rented'")
    rented_books = [row[0] for row in cursor.fetchall()]
    
    # ดึงข้อมูล customers ที่ active
    cursor.execute("SELECT CustomerId FROM Customers WHERE Status = 'Active'")
    active_customers = [row[0] for row in cursor.fetchall()]
    
    if not rented_books or not active_customers:
        print("ไม่พบข้อมูล books หรือ customers สำหรับสร้าง rentals")
        return
    
    rentals = []
    rental_id = 1
    
    # สร้าง active rentals สำหรับหนังสือที่กำลังถูกเช่า
    for book_id in rented_books:
        customer_id = random.choice(active_customers)
        rental_date = datetime.now() - timedelta(days=random.randint(1, 10))
        due_date = rental_date + timedelta(days=7)  # เช่า 7 วัน
        rental_days = 7
        rental_fee = random.uniform(10, 20)
        fine_amount = 0
        
        # ถ้าเกินกำหนดจะมีค่าปรับ
        if datetime.now() > due_date:
            days_late = (datetime.now() - due_date).days
            fine_amount = days_late * 10  # ค่าปรับ 10 บาทต่อวัน
        
        total_amount = rental_fee + fine_amount
        
        rentals.append((
            rental_id, customer_id, book_id, rental_date.strftime('%Y-%m-%d %H:%M:%S'),
            due_date.strftime('%Y-%m-%d %H:%M:%S'), rental_days, rental_fee,
            fine_amount, total_amount, 'Active', 1, None, None
        ))
        rental_id += 1
    
    # สร้าง completed rentals (คืนแล้ว)
    for _ in range(20):  # สร้าง 20 รายการที่คืนแล้ว
        customer_id = random.choice(active_customers)
        book_id = random.randint(1, 15)  # เลือกหนังสือแบบสุ่ม
        rental_date = datetime.now() - timedelta(days=random.randint(15, 60))
        due_date = rental_date + timedelta(days=7)
        return_date = rental_date + timedelta(days=random.randint(3, 12))
        rental_days = 7
        rental_fee = random.uniform(10, 20)
        
        # คำนวณค่าปรับถ้าคืนช้า
        fine_amount = 0
        if return_date > due_date:
            days_late = (return_date - due_date).days
            fine_amount = days_late * 10
        
        total_amount = rental_fee + fine_amount
        
        rentals.append((
            rental_id, customer_id, book_id, rental_date.strftime('%Y-%m-%d %H:%M:%S'),
            due_date.strftime('%Y-%m-%d %H:%M:%S'), rental_days, rental_fee,
            fine_amount, total_amount, 'Completed', 1, return_date.strftime('%Y-%m-%d %H:%M:%S'),
            'คืนแล้ว' if fine_amount == 0 else f'คืนช้า {int(fine_amount/10)} วัน'
        ))
        rental_id += 1
    
    # บันทึกข้อมูล
    for rental in rentals:
        cursor.execute('''
            INSERT OR REPLACE INTO Rentals 
            (RentalId, CustomerId, BookId, RentalDate, DueDate, RentalDays, RentalFee,
             FineAmount, TotalAmount, Status, StaffId, ReturnDate, Notes)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        ''', rental)
    
    print(f"เพิ่มข้อมูล Rentals จำนวน {len(rentals)} รายการ")

def seed_fines(conn):
    """เพิ่มข้อมูล Fines"""
    cursor = conn.cursor()
    
    # ดึงข้อมูล rentals ที่มีค่าปรับ
    cursor.execute('''
        SELECT RentalId, CustomerId, FineAmount 
        FROM Rentals 
        WHERE FineAmount > 0
    ''')
    fine_rentals = cursor.fetchall()
    
    fines = []
    fine_id = 1
    
    for rental_id, customer_id, fine_amount in fine_rentals:
        days_late = int(fine_amount / 10)  # คำนวณจำนวนวันที่เกิน
        fine_rate = 10.00  # ค่าปรับ 10 บาทต่อวัน
        
        # สุ่มว่าจ่ายค่าปรับแล้วหรือยัง
        is_paid = random.choice([True, False, False])  # 33% จ่ายแล้ว
        
        if is_paid:
            paid_amount = fine_amount
            remaining = 0
            status = 'Paid'
            paid_date = (datetime.now() - timedelta(days=random.randint(1, 5))).strftime('%Y-%m-%d %H:%M:%S')
            staff_id = random.choice([1, 2])
        else:
            paid_amount = 0
            remaining = fine_amount
            status = 'Unpaid'
            paid_date = None
            staff_id = None
        
        fines.append((
            fine_id, rental_id, customer_id, f'คืนหนังสือช้า {days_late} วัน',
            days_late, fine_rate, fine_amount, paid_amount, remaining,
            status, datetime.now().strftime('%Y-%m-%d %H:%M:%S'), paid_date, staff_id
        ))
        fine_id += 1
    
    # บันทึกข้อมูล
    for fine in fines:
        cursor.execute('''
            INSERT OR REPLACE INTO Fines 
            (FineId, RentalId, CustomerId, FineReason, DaysLate, FineRate, FineAmount,
             PaidAmount, Remaining, Status, CreatedDate, PaidDate, StaffId)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        ''', fine)
    
    print(f"เพิ่มข้อมูล Fines จำนวน {len(fines)} รายการ")

def seed_daily_reports(conn):
    """เพิ่มข้อมูล Daily Reports"""
    cursor = conn.cursor()
    
    reports = []
    base_date = datetime(2024, 6, 20)  # เริ่มต้น 7 วันที่แล้ว
    
    for i in range(7):  # สร้างรายงาน 7 วันย้อนหลัง
        report_date = (base_date + timedelta(days=i)).strftime('%Y-%m-%d')
        staff_id = random.choice([1, 2])
        
        # สุ่มข้อมูลรายงาน
        total_rentals = random.randint(5, 25)
        total_returns = random.randint(3, 20)
        total_revenue = random.uniform(200, 1500)
        total_fines = random.uniform(0, 300)
        new_customers = random.randint(0, 5)
        created_time = f"{report_date} 23:59:59"
        
        reports.append((
            report_date, staff_id, total_rentals, total_returns,
            total_revenue, total_fines, new_customers, created_time
        ))
    
    # บันทึกข้อมูล
    for report in reports:
        cursor.execute('''
            INSERT OR REPLACE INTO DailyReports 
            (ReportDate, StaffId, TotalRentals, TotalReturns, TotalRevenue,
             TotalFines, NewCustomers, CreatedTime)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?)
        ''', report)
    
    print(f"เพิ่มข้อมูล Daily Reports จำนวน {len(reports)} รายการ")

def main():
    """ฟังก์ชันหลัก"""
    print("🚀 เริ่มต้นการเพิ่มข้อมูล mock ลงใน Comic Rental Database")
    print("=" * 60)
    
    try:
        conn = connect_db()
        
        # เพิ่มข้อมูลตามลำดับ
        seed_customers(conn)
        seed_books(conn)
        seed_rentals(conn)
        seed_fines(conn)
        seed_daily_reports(conn)
        
        # Commit การเปลี่ยนแปลง
        conn.commit()
        conn.close()
        
        print("=" * 60)
        print("✅ เพิ่มข้อมูล mock สำเร็จแล้ว!")
        print("📊 ข้อมูลที่เพิ่ม:")
        print("   - Customers: 15 รายการ")
        print("   - Books: 36 รายการ")
        print("   - Rentals: ~25 รายการ")
        print("   - Fines: ขึ้นอยู่กับ rentals ที่มีค่าปรับ")
        print("   - Daily Reports: 7 รายการ")
        
    except Exception as e:
        print(f"❌ เกิดข้อผิดพลาด: {e}")

if __name__ == "__main__":
    main()