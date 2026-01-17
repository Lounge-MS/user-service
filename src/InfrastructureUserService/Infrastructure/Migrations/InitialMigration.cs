using FluentMigrator;
using FluentMigrator.Postgres;

namespace InfrastructureUserService.Infrastructure.Migrations;

[Migration(202401150001)]
public class InitialMigration : Migration
{
    public override void Up()
    {
        Create.Table("users")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("username").AsString(100).NotNullable()
            .WithColumn("phone_number").AsString(20).NotNullable()
            .WithColumn("password_hash").AsString(255).NotNullable()
            .WithColumn("registered_at").AsDateTime().NotNullable()
            .WithColumn("is_blocked").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("loyalty_points").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("role").AsInt32().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

        Execute.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

        Create.Index("idx_users_username")
            .OnTable("users")
            .OnColumn("username")
            .Ascending()
            .WithOptions();

        Create.Index("idx_users_phone")
            .OnTable("users")
            .OnColumn("phone_number")
            .Ascending();

        Create.Index("idx_users_is_blocked")
            .OnTable("users")
            .OnColumn("is_blocked")
            .Ascending();

        Create.Index("idx_users_role")
            .OnTable("users")
            .OnColumn("role")
            .Ascending();

        Create.Index("idx_users_registered_at")
            .OnTable("users")
            .OnColumn("registered_at")
            .Descending();

        Execute.Sql(@"
            CREATE OR REPLACE FUNCTION update_updated_at_column()
            RETURNS TRIGGER AS $$
            BEGIN
                NEW.updated_at = NOW();
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
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("user_id").AsGuid().NotNullable()
            .ForeignKey("fk_points_history_user_id", "users", "id")
            .OnDelete(System.Data.Rule.Cascade)
            .WithColumn("order_id").AsGuid().Nullable()
            .WithColumn("points").AsInt32().NotNullable()
            .WithColumn("type").AsInt32().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithDefault(SystemMethods.CurrentDateTime);

        Create.Index("idx_points_history_user_id")
            .OnTable("points_history")
            .OnColumn("user_id")
            .Ascending();

        Create.Index("idx_points_history_created_at")
            .OnTable("points_history")
            .OnColumn("created_at")
            .Descending();

        Create.Index("idx_points_history_order_id")
            .OnTable("points_history")
            .OnColumn("order_id")
            .Ascending()
            .NullsLast();

        Create.Index("idx_points_history_type")
            .OnTable("points_history")
            .OnColumn("type")
            .Ascending();

        Create.Table("system_constants")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("key").AsString(100).NotNullable().Unique()
            .WithColumn("value").AsString(500).NotNullable()
            .WithColumn("description").AsString(500).Nullable()
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithDefault(SystemMethods.CurrentDateTime);

        Execute.Sql(@"
            INSERT INTO system_constants (id, key, value, description)
            VALUES
                (gen_random_uuid(), 'DEFAULT_USER_ROLE', '0', 'Роль по умолчанию для новых пользователей (0 = GUEST)')
        ");

        // Seed admin user (password: admin123)
        // BCrypt hash for 'admin123': $2a$11$K3g.../...
        Execute.Sql(@"
            INSERT INTO users (id, username, phone_number, password_hash, registered_at, is_blocked, loyalty_points, role)
            VALUES
                (gen_random_uuid(), 'admin', '+70000000000', '$2a$11$rBXKQq8Y9YuJZ5qK5u.ZPOqL3VjK1VxUxM6k8rZqJ5Yx8G3d5q8Ky', NOW(), false, 0, 6)
        ");

        Create.Table("user_audit_logs")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("user_id").AsGuid().NotNullable()
            .ForeignKey("fk_audit_logs_user_id", "users", "id")
            .WithColumn("action").AsString(100).NotNullable()
            .WithColumn("details").AsString().Nullable()
            .WithColumn("ip_address").AsString(45).Nullable()
            .WithColumn("user_agent").AsString(500).Nullable()
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithDefault(SystemMethods.CurrentDateTime);

        Create.Index("idx_audit_logs_user_id")
            .OnTable("user_audit_logs")
            .OnColumn("user_id")
            .Ascending();

        Create.Index("idx_audit_logs_created_at")
            .OnTable("user_audit_logs")
            .OnColumn("created_at")
            .Descending();

        Execute.Sql(@"
            CREATE OR REPLACE VIEW user_statistics AS
            SELECT 
                u.id,
                u.username,
                u.phone_number,
                u.registered_at,
                u.is_blocked,
                u.loyalty_points,
                u.role,
                COUNT(ph.id) as TotalTransactions,
                COALESCE(SUM(CASE WHEN ph.type = 0 THEN ph.points ELSE 0 END), 0) as TotalAccrued,
                COALESCE(SUM(CASE WHEN ph.type = 1 THEN ph.points ELSE 0 END), 0) as TotalDeducted
            FROM users u
            LEFT JOIN points_history ph ON u.id = ph.user_id
            GROUP BY u.id, u.username, u.phone_number, u.registered_at, u.is_blocked, u.loyalty_points, u.role;
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