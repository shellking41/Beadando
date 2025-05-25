using Microsoft.EntityFrameworkCore;
using Beadando.Models;

namespace Beadando.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<UserQuizResult> UserQuizResults { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Question - Answer relationship
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - Session relationship
            modelBuilder.Entity<Session>()
                .HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserQuizResult relationships
            modelBuilder.Entity<UserQuizResult>()
                .HasOne(r => r.User)
                .WithMany(u => u.QuizResults)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserAnswer relationships
            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.UserQuizResult)
                .WithMany(r => r.UserAnswers)
                .HasForeignKey(ua => ua.UserQuizResultId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.Question)
                .WithMany()
                .HasForeignKey(ua => ua.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.Answer)
                .WithMany(a => a.UserAnswers)
                .HasForeignKey(ua => ua.AnswerId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
} 