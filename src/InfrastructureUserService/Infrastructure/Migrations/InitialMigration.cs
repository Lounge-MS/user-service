using FluentMigrator;
using FluentMigrator.Postgres;

namespace InfrastructureUserService.Infrastructure.Migrations;

[Migration(202401150001)]
public class InitialMigration : Migration
{
    public override void Up()
    {
        Create.Table("users")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("Username").AsString(100).NotNullable()
            .WithColumn("PhoneNumber").AsString(20).NotNullable()
            .WithColumn("PasswordHash").AsString(255).NotNullable()
            .WithColumn("RegisteredAt").AsDateTime().NotNullable()
            .WithColumn("IsBlocked").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("LoyaltyPoints").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("Role").AsInt32().NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("UpdatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

        Execute.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

        Create.Index("idx_users_username")
            .OnTable("users")
            .OnColumn("Username")
            .Ascending()
            .WithOptions();

        Create.Index("idx_users_phone")
            .OnTable("users")
            .OnColumn("PhoneNumber")
            .Ascending();

        Create.Index("idx_users_is_blocked")
            .OnTable("users")
            .OnColumn("IsBlocked")
            .Ascending();

        Create.Index("idx_users_role")
            .OnTable("users")
            .OnColumn("Role")
            .Ascending();

        Create.Index("idx_users_registered_at")
            .OnTable("users")
            .OnColumn("RegisteredAt")
            .Descending();

        Execute.Sql(@"
            CREATE OR REPLACE FUNCTION update_updated_at_column()
            RETURNS TRIGGER AS $$
            BEGIN
                NEW.""UpdatedAt"" = NOW();
                RETURN NEW;
            END;
            $$ language 'plpgsql';
        ");

        Execute.Sql(@"
            CREATE TRIGGER update_users_updated_at
            BEFORE UPDATE ON users
            FOR EACH ROW
            EXECUTE FUNCTION update_updated_at_column();
        ");

        Create.Table("points_history")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("UserId").AsGuid().NotNullable()
            .ForeignKey("fk_points_history_user_id", "users", "Id")
            .OnDelete(System.Data.Rule.Cascade)
            .WithColumn("OrderId").AsGuid().Nullable()
            .WithColumn("Points").AsInt32().NotNullable()
            .WithColumn("Type").AsInt32().NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithDefault(SystemMethods.CurrentDateTime);

        Create.Index("idx_points_history_user_id")
            .OnTable("points_history")
            .OnColumn("UserId")
            .Ascending();

        Create.Index("idx_points_history_created_at")
            .OnTable("points_history")
            .OnColumn("CreatedAt")
            .Descending();

        Create.Index("idx_points_history_order_id")
            .OnTable("points_history")
            .OnColumn("OrderId")
            .Ascending()
            .NullsLast();

        Create.Index("idx_points_history_type")
            .OnTable("points_history")
            .OnColumn("Type")
            .Ascending();

        Create.Table("system_constants")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("Key").AsString(100).NotNullable().Unique()
            .WithColumn("Value").AsString(500).NotNullable()
            .WithColumn("Description").AsString(500).Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithDefault(SystemMethods.CurrentDateTime);

        Execute.Sql(@"
            INSERT INTO system_constants (Id, Key, Value, Description)
            VALUES 
                (gen_random_uuid(), 'DEFAULT_USER_ROLE', '0', 'Роль по умолчанию для новых пользователей (0 = GUEST)')
        ");

        Create.Table("user_audit_logs")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("UserId").AsGuid().NotNullable()
            .ForeignKey("fk_audit_logs_user_id", "users", "Id")
            .WithColumn("Action").AsString(100).NotNullable()
            .WithColumn("Details").AsString().Nullable()
            .WithColumn("IpAddress").AsString(45).Nullable()
            .WithColumn("UserAgent").AsString(500).Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithDefault(SystemMethods.CurrentDateTime);

        Create.Index("idx_audit_logs_user_id")
            .OnTable("user_audit_logs")
            .OnColumn("UserId")
            .Ascending();

        Create.Index("idx_audit_logs_created_at")
            .OnTable("user_audit_logs")
            .OnColumn("CreatedAt")
            .Descending();

        Execute.Sql(@"
            CREATE OR REPLACE VIEW user_statistics AS
            SELECT 
                u.Id,
                u.Username,
                u.PhoneNumber,
                u.RegisteredAt,
                u.IsBlocked,
                u.LoyaltyPoints,
                u.Role,
                COUNT(ph.Id) as TotalTransactions,
                COALESCE(SUM(CASE WHEN ph.Type = 0 THEN ph.Points ELSE 0 END), 0) as TotalAccrued,
                COALESCE(SUM(CASE WHEN ph.Type = 1 THEN ph.Points ELSE 0 END), 0) as TotalDeducted
            FROM users u
            LEFT JOIN points_history ph ON u.Id = ph.UserId
            GROUP BY u.Id, u.Username, u.PhoneNumber, u.RegisteredAt, u.IsBlocked, u.LoyaltyPoints, u.Role;
        ");
    }

    public override void Down()
    {
        Execute.Sql("DROP VIEW IF EXISTS user_statistics;");

        Delete.Index("idx_audit_logs_created_at").OnTable("user_audit_logs");
        Delete.Index("idx_audit_logs_user_id").OnTable("user_audit_logs");
        Delete.Table("user_audit_logs");

        Delete.Table("system_constants");

        Delete.Index("idx_points_history_type").OnTable("points_history");
        Delete.Index("idx_points_history_order_id").OnTable("points_history");
        Delete.Index("idx_points_history_created_at").OnTable("points_history");
        Delete.Index("idx_points_history_user_id").OnTable("points_history");
        Delete.Table("points_history");

        Delete.Index("idx_users_registered_at").OnTable("users");
        Delete.Index("idx_users_role").OnTable("users");
        Delete.Index("idx_users_is_blocked").OnTable("users");
        Delete.Index("idx_users_phone").OnTable("users");
        Delete.Index("idx_users_username").OnTable("users");

        Delete.Table("users");

        Execute.Sql("DROP EXTENSION IF EXISTS pg_trgm;");
    }
}