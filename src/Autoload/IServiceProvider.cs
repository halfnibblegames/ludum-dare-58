using System.Diagnostics.CodeAnalysis;

namespace HalfNibbleGame.Autoload;

public interface IServiceProvider {
  void ProvidePersistent<T>(T obj);
  void ProvideInScene<T>(T obj);
  T Get<T>();
  bool TryGet<T>([NotNullWhen(true)] out T? obj);
}
