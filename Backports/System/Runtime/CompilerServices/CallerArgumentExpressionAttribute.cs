3938b165a478fd9d5942ab4906aa512e5b4f572b
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       ons is distributed in the hope that
//     it will be useful, but WITHOUT ANY WARRANTY; without even the implied
//     warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
//     the GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with Hawkynt's .NET Framework extensions.
//     If not, see <http://www.gnu.org/licenses/>.
// */
// #endregion

namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class CallerArgumentExpressionAttribute : Attribute {
  public string ParameterName { get; }
  public CallerArgumentExpressionAttribute(string parameterName) => this.ParameterName = parameterName;
}

#endif