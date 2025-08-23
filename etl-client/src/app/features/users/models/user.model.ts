export type User = {
    id: string;
    username: string;
    password: string;
    email: string;
    role: "user" | "admin" | "sysAdmin";
};

export type UserRow = Omit<User, "password">;

export type TableColumn<T> = {
    key: keyof T;
    label: string;
};
