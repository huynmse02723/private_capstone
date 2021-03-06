﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DDL_CapstoneProject.Helper;
using DDL_CapstoneProject.Helpers;
using DDL_CapstoneProject.Models;
using DDL_CapstoneProject.Models.DTOs;
using DDL_CapstoneProject.Ultilities;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Diagnostics;
using WebGrease.Css.Extensions;

namespace DDL_CapstoneProject.Respository
{
    public class ProjectRepository : SingletonBase<ProjectRepository>
    {

        #region "Constructors"
        private ProjectRepository()
        {
        }
        #endregion

        #region "Methods"


        #region TrungVN


        public List<ProjectExportDTO> AdminGetAllProjectDetail()
        {
            using (var db = new DDLDataContext())
            {
                List<ProjectExportDTO> list = new List<ProjectExportDTO>();
                var projects = (from project in db.Projects
                                where project.Status != DDLConstants.ProjectStatus.DRAFT
                                orderby project.CreatedDate ascending
                                select new ProjectExportDTO
                                {
                                    ProjectCode = project.ProjectCode,
                                    CreatedDate = project.CreatedDate,
                                    Title = project.Title,
                                    CurrentFunded = project.CurrentFunded + "",
                                    FundingGoal = project.FundingGoal + "",
                                    ExpireDate = project.ExpireDate,
                                    Status = project.Status == "approved" ? "Đang chạy" :
                                                (project.Status == "rejected" ? "Từ chối" :
                                                (project.Status == "expired" ? "Hết hạn" :
                                                (project.Status == "pending" ? "Chờ duyệt" :
                                                (project.Status == "suspended" ? "Đình chỉ" : "")))),
                                    IsFunded = project.IsFunded ? "Đã đủ tiền" : "Chưa đủ tiền",
                                    IsExprired = project.IsExprired ? "Đã quá hạn" : "Chưa quá hạn",
                                    NumberBacked = project.Backings.Count + "",
                                    CategoryName = project.Category.Name,
                                    Location = project.Location,
                                    NumberUpdate = project.UpdateLogs.Count + "",
                                    CreatorFullname = project.Creator.UserInfo.FullName,
                                    CreatorUserName = project.Creator.Username,
                                }).ToList();
                return projects;
            }
        }

        public List<ProjectTitleDTO> projectTitleList(string searchkey)
        {
            if (searchkey == null) searchkey = "";
            List<ProjectTitleDTO> list = new List<ProjectTitleDTO>();
            using (var db = new DDLDataContext())
            {
                var listTitle = from project in db.Projects
                                where project.Title.Contains(searchkey)
                                && !project.Status.Equals(DDLConstants.ProjectStatus.DRAFT)
                                && !project.Status.Equals(DDLConstants.ProjectStatus.REJECTED)
                                && !project.Status.Equals(DDLConstants.ProjectStatus.SUSPENDED)
                                && !project.Status.Equals(DDLConstants.ProjectStatus.PENDING)
                                select new ProjectTitleDTO
                                {
                                    projectTitle = project.Title
                                };
                list = listTitle.ToList();
            }

            return list;
        }


        public StatisticDTO getStatisticsInfor()
        {
            using (var db = new DDLDataContext())
            {
                //Dictionary<string, int> dic = new Dictionary<string, int>();
                //projectSuccesedCount
                int projectSuccesedCount = GetProject(0, 0, "All", "", "", "", "true", "true").Count;
                //total funded
                var totalFund = from project in db.Projects
                                select project.CurrentFunded;
                //backingusercount and userbackmuchcount 
                var backingUser = from backing in db.Backings
                                  group backing by backing.UserID
                                      into b
                                      select b;
                int backingUserCount = backingUser.Count();
                var backingUserCount2 = (from backing in backingUser
                                         select backing.GroupBy(x => x.ProjectID)).ToList();
                List<int> UserBackmuchList = new List<int>();
                foreach (var item in backingUserCount2)
                {
                    if (item.Count() >= 2)
                        UserBackmuchList.Add(item.Count());
                }
                int UserBackmuchCount = UserBackmuchList.Count();

                //numberofbacking
                int NumberOfBacking = (from backing in db.Backings
                                       select backing.BackingID).Count();

                var statistic = new StatisticDTO
                {
                    SuccesedCount = projectSuccesedCount,
                    TotalFunded = totalFund.Sum(),
                    BackingUserCount = backingUserCount,
                    NumberOfBacking = NumberOfBacking,
                    UserBackmuchCount = UserBackmuchCount
                };
                //dic.Add("SuccesedCount", projectSuccesedCount);
                //dic.Add("TotalFunded", Convert.ToDecimal(totalFund.Sum());
                //dic.Add("BackingUserCount", backingUserCount);
                //dic.Add("UserBackmuchCount", UserBackmuchCount);
                //dic.Add("NumberOfBacking", NumberOfBacking);
                return statistic;
            }
        }


        public List<ProjectBasicViewDTO> GetProjectTop(String categoryid)
        {
            categoryid = "|" + categoryid + "|";
            return GetProject(10, 0, categoryid, "CurrentFunded", "", "", "", "true");
        }

        public int SearchCount(string categoryidlist, string searchkey, string statusString)
        {
            int searchcount = GetProject(0, 0, categoryidlist, "", searchkey, "", statusString, "").Count;
            return searchcount;
        }



        public List<ProjectBasicViewDTO> GetProject(int take, int from, String categoryidList, string order,
                                                    string pathofprojectname, string status,
                                                    string isExprired, string isFunded)
        {
            using (var db = new DDLDataContext())
            {
                if (pathofprojectname == null)
                {
                    pathofprojectname = "";
                }
                else
                {
                    if (pathofprojectname == "null")
                        pathofprojectname = "";
                }
                if (status == null) status = "";
                if (isFunded == null) isFunded = "";
                if (isExprired == null) isExprired = "";
                int CategoryExistCount = (from category in db.Categories
                                          where (categoryidList.ToLower().Contains("all") ||
                                                 categoryidList.Contains("|" + category.CategoryID + "|")) && category.IsActive
                                          select new CategoryDTO { }).Count();
                if (CategoryExistCount == 0) throw new DllNotFoundException();
                var ProjectList = from project in db.Projects
                                  where
                                      (categoryidList.ToLower().Contains("all") ||
                                       categoryidList.Contains("|" + project.CategoryID + "|"))
                                      && (project.IsExprired + "").Contains(isExprired) && project.Title.Contains(pathofprojectname)
                                      && project.Status.Contains(status) && project.IsFunded.ToString().ToLower().Contains(isFunded)
                                      && !project.Status.Equals(DDLConstants.ProjectStatus.DRAFT) && !project.Status.Equals(DDLConstants.ProjectStatus.REJECTED)
                                      && !project.Status.Equals(DDLConstants.ProjectStatus.SUSPENDED) && !project.Status.Equals(DDLConstants.ProjectStatus.PENDING)
                                  select new ProjectBasicViewDTO
                                  {
                                      ProjectID = project.ProjectID,
                                      ProjectCode = project.ProjectCode,
                                      CreatorName = project.Creator.UserInfo.FullName,
                                      Title = project.Title,
                                      ImageUrl = project.ImageUrl,
                                      SubDescription = project.SubDescription,
                                      Location = project.Location,
                                      CurrentFunded = Decimal.Round((project.CurrentFunded / project.FundingGoal) * 100),
                                      CurrentFundedNumber = project.CurrentFunded,
                                      ExpireDate = DbFunctions.DiffDays(DateTime.UtcNow, project.ExpireDate),
                                      FundingGoal = project.FundingGoal,
                                      Category = project.Category.Name,
                                      CategoryID = project.CategoryID,
                                      Backers = project.Backings.Count,
                                      CreatedDate = project.CreatedDate,
                                      PopularPoint = project.PopularPoint
                                  };

                if (from >= ProjectList.Count())
                {
                    return new List<ProjectBasicViewDTO>();
                }
                if (take == 0) take = ProjectList.Count();
                var listProject = orderBy(order, ProjectList).Take(take + from).ToList();
                listProject.RemoveRange(0, from);
                // Convert datetime to GMT+7
                listProject.ForEach(x => x.CreatedDate = CommonUtils.ConvertDateTimeFromUtc(x.CreatedDate));
                return listProject;
            }
        }


        private IQueryable<ProjectBasicViewDTO> orderBy(string order, IQueryable<ProjectBasicViewDTO> ProjectList)
        {
            using (var db = new DDLDataContext())
            {
                if (order == "CurrentFunded")
                {
                    return from project in ProjectList
                           orderby project.CurrentFundedNumber descending
                           select project;
                }
                if (order == "CreatedDate")
                {
                    return from project in ProjectList
                           orderby project.CreatedDate descending
                           select project;
                }
                if (order == "PopularPoint")
                {
                    return from project in ProjectList
                           orderby project.PopularPoint descending
                           select project;
                }
                if (order == "ExpireDate")
                {
                    return from project in ProjectList
                           orderby project.ExpireDate
                           select project;
                }
                if (order == "FundingGoal")
                {
                    return from project in ProjectList
                           orderby project.FundingGoal
                           select project;
                }
                return ProjectList;
            }
        }
        public List<ProjectBasicViewDTO> GetProjectByCategory()
        {
            using (var db = new DDLDataContext())
            {
                var projectList = new List<ProjectBasicViewDTO>();
                var cat = db.Categories.Where(x => x.IsActive).Select(x => x.CategoryID).ToList();
                for (int i = 0; i < cat.Count; i++)
                {
                    var getProject = GetProject(1, 0, "|" + cat[i] + "|",
                        "PopularPoint", "", "", "false", "");
                    if (getProject.Any())
                        projectList.Add(getProject[0]);
                }
                return projectList;
            }
        }


        public List<List<ProjectBasicViewDTO>> GetProjectStatisticList()
        {
            var projectList = new List<List<ProjectBasicViewDTO>>
            {
                GetProject(4, 0, "All", "PopularPoint", "", "", "false", ""),
                GetProject(4, 0, "All", "CreatedDate", "", "", "false", ""),
                GetProject(4, 0, "All", "CurrentFunded", "", "", "false", ""),
                GetProject(4, 0, "All", "ExpireDate", "", "", "false", "")
            };

            return projectList;
        }
        public Dictionary<string, List<ProjectBasicViewDTO>> GetStatisticListForHome()
        {
            var projectList = new Dictionary<string, List<ProjectBasicViewDTO>>
            {
                {"popularproject", GetProject(4, 0, "All", "PopularPoint", "", "", "false", "")},
                {"projectByCategory", GetProjectByCategory()},
                {"highestprojectpledge", GetProject(1, 0, "All", "CurrentFunded", "", "", "false", "")},
                {"highestprojectfund", GetProject(1, 0, "All", "CurrentFunded", "", "", "true", "true")},
                {"totalprojectfund", GetTotalFund()}
            };
            return projectList;
        }

        public List<ProjectBasicViewDTO> GetTotalFund()
        {
            using (var db = new DDLDataContext())
            {
                var list = new List<ProjectBasicViewDTO>();
                var TotalFund = from project in db.Projects
                                select project.CurrentFunded;
                var totalFund = new ProjectBasicViewDTO
                {
                    CurrentFundedNumber = (Convert.ToDecimal(TotalFund.Sum()))
                };

                list.Add(totalFund);
                return list;
            }
        }
        #endregion

        #region HuyNM
        /// <summary>
        /// Initial a empty project
        /// </summary>
        /// <returns>emptyProject</returns>
        public Project CreateEmptyProject()
        {
            using (var db = new DDLDataContext())
            {
                // Create project 
                var project = db.Projects.Create();
                project.ProjectCode = string.Empty;
                project.CategoryID = 0;
                project.CreatorID = 0;
                project.Title = string.Empty;
                project.CreatedDate = DateTime.UtcNow;
                project.UpdatedDate = DateTime.UtcNow;
                project.Risk = string.Empty;
                project.ImageUrl = "default_img.png";
                project.SubDescription = string.Empty;
                project.Location = string.Empty;
                project.IsExprired = false;
                project.CurrentFunded = 0;
                project.FundingGoal = 0;
                project.Description = string.Empty;
                project.VideoUrl = string.Empty;
                project.PopularPoint = 0;
                project.Status = DDLConstants.ProjectStatus.DRAFT;
                project.PointOfTheDay = 0;

                return project;
            }
        }

        /// <summary>
        /// Create a new project
        /// </summary>
        /// <returns>project</returns>
        public string CreatProject(ProjectCreateDTO newProject, string username)
        {
            using (var db = new DDLDataContext())
            {
                var user = db.DDL_Users.SingleOrDefault(x => x.Username == username);
                if (user == null)
                {
                    throw new UserNotFoundException();
                }

                var project = CreateEmptyProject();
                project.CreatorID = newProject.CategoryID;
                project.Title = newProject.Title;
                project.CategoryID = newProject.CategoryID;
                project.CreatorID = user.DDL_UserID;
                project.FundingGoal = newProject.FundingGoal;

                db.Projects.Add(project);
                db.SaveChanges();

                // Create projectCode
                string projectIDString = project.ProjectID.ToString();
                int plusLength = 6 - projectIDString.Length;
                string plusCode = string.Concat(Enumerable.Repeat("0", plusLength));
                project.ProjectCode = "PRJ" + plusCode + projectIDString;

                db.SaveChanges();

                return project.ProjectCode;
            }
        }

        /// <summary>
        /// Edit a project
        /// </summary>
        /// <param name="project">object</param>
        /// <returns>updateProject</returns>
        public ProjectEditDTO EditProjectBasic(ProjectEditDTO project, string uploadImageName, string username)
        {
            using (var db = new DDLDataContext())
            {
                var updateProject = db.Projects.SingleOrDefault(u => u.ProjectID == project.ProjectID);

                if (updateProject == null)
                {
                    throw new KeyNotFoundException();
                }

                if (username != updateProject.Creator.Username)
                {
                    throw new NotPermissionException();
                }

                if (uploadImageName != string.Empty)
                {
                    updateProject.ImageUrl = uploadImageName;
                }

                updateProject.CategoryID = project.CategoryID;
                updateProject.ExpireDate = project.ExpireDate;
                updateProject.FundingGoal = project.FundingGoal;
                updateProject.Location = project.Location;
                updateProject.Status = project.Status;
                updateProject.SubDescription = project.SubDescription;
                updateProject.Title = project.Title;
                updateProject.UpdatedDate = DateTime.UtcNow;

                if (updateProject.ExpireDate != null)
                {
                    TimeSpan ts = new TimeSpan(23, 55, 0);
                    updateProject.ExpireDate = updateProject.ExpireDate.GetValueOrDefault().Date + ts;

                    updateProject.ExpireDate =
                        CommonUtils.ConvertDateTimeToUtc(updateProject.ExpireDate.GetValueOrDefault());
                }

                db.SaveChanges();

                var updateProjectDTO = new ProjectEditDTO
                {
                    CategoryID = updateProject.CategoryID,
                    ExpireDate = updateProject.ExpireDate,
                    FundingGoal = updateProject.FundingGoal,
                    Location = updateProject.Location,
                    Status = updateProject.Status,
                    SubDescription = updateProject.SubDescription,
                    Title = updateProject.Title,
                    ProjectID = updateProject.ProjectID,
                    ProjectCode = updateProject.ProjectCode,
                    ImageUrl = updateProject.ImageUrl,
                    CurrentFunded = project.CurrentFunded,
                };

                if (updateProjectDTO.ExpireDate != null)
                {
                    updateProjectDTO.ExpireDate =
                    CommonUtils.ConvertDateTimeFromUtc(updateProjectDTO.ExpireDate.GetValueOrDefault());
                }

                // Set number exprire day.
                var timespan = updateProjectDTO.ExpireDate - CommonUtils.DateTodayGMT7();
                updateProjectDTO.NumberDays = timespan.GetValueOrDefault().Days;

                return updateProjectDTO;
            }
        }

        /// <summary>
        /// Edit project's story
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public ProjectStoryDTO EditProjectStory(ProjectStoryDTO project, string username)
        {
            using (var db = new DDLDataContext())
            {
                var updateProject = db.Projects.SingleOrDefault(u => u.ProjectID == project.ProjectID);

                if (updateProject == null)
                {
                    throw new KeyNotFoundException();
                }

                if (username != updateProject.Creator.Username)
                {
                    throw new NotPermissionException();
                }

                updateProject.Risk = project.Risk.Trim();
                updateProject.VideoUrl = project.VideoUrl;
                updateProject.Description = project.Description.Trim();
                updateProject.UpdatedDate = DateTime.UtcNow;

                db.SaveChanges();

                var updateProjectDTO = new ProjectStoryDTO
                {
                    Risk = updateProject.Risk,
                    VideoUrl = updateProject.VideoUrl,
                    Description = updateProject.Description,
                    ProjectID = updateProject.ProjectID
                };

                return updateProjectDTO;
            }
        }

        /// <summary>
        /// Get a project
        /// </summary>
        /// <param name="ProjectID">int</param>
        /// <returns>project</returns>
        public ProjectEditDTO GetProjectBasic(string code, string UserName)
        {
            using (var db = new DDLDataContext())
            {
                var project = db.Projects.SingleOrDefault(x => x.ProjectCode == code);

                if (project == null)
                {
                    throw new KeyNotFoundException();
                }

                if (project.Creator.Username != UserName)
                {
                    throw new NotPermissionException();
                }

                var projectDTO = new ProjectEditDTO
                {
                    ProjectID = project.ProjectID,
                    ProjectCode = project.ProjectCode,
                    CategoryID = project.CategoryID,
                    Title = project.Title,
                    ImageUrl = project.ImageUrl,
                    SubDescription = project.SubDescription,
                    Location = project.Location,
                    ExpireDate = project.ExpireDate,
                    FundingGoal = project.FundingGoal,
                    Status = project.Status,
                    CurrentFunded = project.CurrentFunded,
                };

                if (projectDTO.ExpireDate != null)
                {
                    projectDTO.ExpireDate = CommonUtils.ConvertDateTimeFromUtc(projectDTO.ExpireDate.GetValueOrDefault());
                }

                // Set number exprire day.
                var timespan = projectDTO.ExpireDate - CommonUtils.DateTodayGMT7();
                projectDTO.NumberDays = timespan.GetValueOrDefault().Days;

                return projectDTO;
            }
        }

        /// <summary>
        /// Get projec story
        /// </summary>
        /// <param name="ProjectID"></param>
        /// <returns>projectBasicDTO</returns>
        public ProjectStoryDTO GetProjectStory(int ProjectID, string username)
        {
            using (var db = new DDLDataContext())
            {
                var project = db.Projects.SingleOrDefault(x => x.ProjectID == ProjectID);

                if (project == null)
                {
                    throw new KeyNotFoundException();
                }

                if (project.Creator.Username != username)
                {
                    throw new NotPermissionException();
                }

                var projectBasicDTO = new ProjectStoryDTO
                {
                    ProjectID = project.ProjectID,
                    Description = project.Description,
                    Risk = project.Risk,
                    VideoUrl = project.VideoUrl
                };

                return projectBasicDTO;
            }
        }

        /// <summary>
        /// Submit a project
        /// </summary>
        /// <param name="submitProject"></param>
        /// <returns>errorList</returns>
        public List<string> SubmitProject(ProjectEditDTO submitProject, string username)
        {
            using (var db = new DDLDataContext())
            {
                var project = db.Projects.SingleOrDefault(u => u.ProjectID == submitProject.ProjectID);

                if (project == null)
                {
                    throw new KeyNotFoundException();
                }

                if (project.Creator.Username != username)
                {
                    throw new NotPermissionException();
                }

                if (project.ExpireDate != null)
                {
                    project.ExpireDate = CommonUtils.ConvertDateTimeFromUtc(project.ExpireDate.GetValueOrDefault());
                }

                List<string> mylist = new List<string>();

                var expireDateLaw = DateTime.UtcNow.AddDays(10);
                expireDateLaw = CommonUtils.ConvertDateTimeFromUtc(expireDateLaw);

                string messageBasic = string.Empty;
                if (string.IsNullOrEmpty(project.Title) || project.Title.Length < 10 || project.Title.Length > 60
                    || string.IsNullOrEmpty(project.ImageUrl) || project.ImageUrl == "default_img.png"
                    || string.IsNullOrEmpty(project.SubDescription) || project.SubDescription.Length < 30 || project.SubDescription.Length > 300
                    || string.IsNullOrEmpty(project.Location) || project.Location.Length < 10 || project.Location.Length > 60
                    || project.ExpireDate == null || project.ExpireDate < expireDateLaw
                    || project.FundingGoal < 1000000)
                {
                    messageBasic = "Xin hãy xem lại trang thông tin cơ bản, các trường (kể cả ảnh dự án) PHẢI được điền đầy đủ và hợp lệ";
                    mylist.Add(messageBasic);

                }

                var rewardList = ProjectRepository.Instance.GetRewardPkg(project.ProjectID);
                rewardList.ForEach(x => x.EstimatedDelivery = CommonUtils.ConvertDateTimeFromUtc(x.EstimatedDelivery.GetValueOrDefault()));
                string messageReward = string.Empty;

                if (rewardList.Count == 0)
                {
                    messageReward = "Xin hãy xem lại trang Mức ủng hộ! Cần ít nhất 1 mức!";
                    mylist.Add(messageReward);
                }

                if (rewardList.Any(reward => reward.PledgeAmount < 10000
                    || string.IsNullOrEmpty(reward.Description) || reward.Description.Length < 10 || reward.Description.Length > 135
                    || (reward.EstimatedDelivery < project.ExpireDate && reward.Type != DDLConstants.RewardType.NO_REWARD)
                    || (reward.Type == DDLConstants.RewardType.LIMITED && reward.Quantity < 1)
                    ))
                {
                    messageReward = "Xin hãy xem lại trang Mức ủng hộ! Tất cả các trường PHẢI được điền đầy đủ và hợp lệ(Ngày giao phải sau ngày đóng gây quỹ)";
                    mylist.Add(messageReward);
                }

                string messageStory = string.Empty;
                if (string.IsNullOrEmpty(project.Description) || project.Description.Length < 135
                    || (!string.IsNullOrEmpty(project.Risk) && project.Risk.Length < 135)
                    )
                {
                    messageStory = "Xin hãy xem lại trang Mô tả chỉ tiết! Các trường PHẢI được nhập đầy đủ và hợp lệ (video và mô tả khó khăn không bắt buộc)";
                    mylist.Add(messageStory);
                }

                if (string.IsNullOrEmpty(messageBasic) && string.IsNullOrEmpty(messageReward) && string.IsNullOrEmpty(messageStory))
                {
                    project.UpdatedDate = DateTime.UtcNow;
                    project.Status = DDLConstants.ProjectStatus.PENDING;
                    project.CreatedDate = DateTime.UtcNow;

                    db.SaveChanges();
                }

                return mylist;
            }
        }

        /// <summary>
        /// Get bacsic information of project
        /// </summary>
        /// <param name="code"></param>
        /// <returns>projectInforDTO</returns>
        public ProjectInfoBackDTO GetBackProjectInfo(string code)
        {
            using (var db = new DDLDataContext())
            {
                var project = db.Projects.SingleOrDefault(x => x.ProjectCode == code);

                if (project == null)
                {
                    throw new KeyNotFoundException();
                }

                var projectInforDTO = new ProjectInfoBackDTO
                {
                    ProjectCode = project.ProjectCode,
                    Title = project.Title,
                    Creator = project.Creator.UserInfo.FullName,
                    CreatorUsername = project.Creator.Username
                };

                return projectInforDTO;
            }
        }

        /// <summary>
        /// Back a project
        /// </summary>
        /// <param name="backingData"></param>
        /// <returns>projectCode</returns>
        public int BackProject(ProjectBackDTO backingData)
        {
            using (var db = new DDLDataContext())
            {
                var project = db.Projects.SingleOrDefault(x => x.ProjectCode == backingData.ProjectCode);

                if (project == null)
                {
                    throw new KeyNotFoundException();
                }

                var user = db.DDL_Users.SingleOrDefault((x => x.Email == backingData.Email));
                if (user == null)
                {
                    throw new UserNotFoundException();
                }

                //backingData.BackedDate = CommonUtils.ConvertDateTimeToUtc(backingData.BackedDate);

                // Create new backing record
                var backing = db.Backings.Create();
                backing.UserID = user.DDL_UserID;
                backing.ProjectID = project.ProjectID;
                backing.BackedDate = backingData.BackedDate;
                //backing.IsPublic = backingData.IsPublic;

                // Create new backingDetail recored
                var backingDetail = db.BackingDetails.Create();
                backingDetail.BackerName = backingData.BackerName;
                backingDetail.RewardPkgID = backingData.RewardPkgID;
                backingDetail.PledgedAmount = backingData.PledgeAmount;
                backingDetail.Quantity = backingData.Quantity;
                backingDetail.Description = backingData.Description;
                backingDetail.Address = backingData.Address;
                backingDetail.Email = backingData.Email;
                backingDetail.PhoneNumber = backingData.PhoneNumber;
                backingDetail.OrderId = backingData.OrderId;
                backingDetail.TransactionId = backingData.TransactionId;

                // Add backingDetail to backing
                backing.BackingDetail = backingDetail;

                db.Backings.Add(backing);

                // Caculate project current fund
                project.CurrentFunded += backingDetail.PledgedAmount;

                // Caculate reward quantity
                var rewardPkg = db.RewardPkgs.SingleOrDefault(u => u.RewardPkgID == backingDetail.RewardPkgID);
                if (rewardPkg == null)
                {
                    throw new KeyNotFoundException();
                }

                rewardPkg.CurrentQuantity += backingDetail.Quantity;

                // Set project is funded
                if ((project.CurrentFunded >= project.FundingGoal) && project.IsFunded == false)
                {
                    project.IsFunded = true;
                }

                db.SaveChanges();

                return backing.BackingID;
            }
        }

        /// <summary>
        /// Caculate point of the day of a project
        /// </summary>
        /// <param name="projectCode"></param>
        /// <param name="point"></param>
        public void CaculateProjectPoint(string projectCode, int point)
        {
            using (var db = new DDLDataContext())
            {
                var project = db.Projects.SingleOrDefault(x => x.ProjectCode == projectCode);

                if (project == null)
                {
                    throw new KeyNotFoundException();
                }

                project.PointOfTheDay += point;

                db.SaveChanges();
            }
        }

        /// <summary>
        /// Get backing detail
        /// </summary>
        /// <param name="backingId"></param>
        /// <returns></returns>
        public ProjectBackDTO GetBackingDetail(int backingId, string username)
        {
            using (var db = new DDLDataContext())
            {
                var backing = db.BackingDetails.SingleOrDefault(x => x.BackingID == backingId);

                if (backing == null)
                {
                    throw new KeyNotFoundException();
                }

                //if (backing.Backing.User.Username != username)
                //{
                //    throw new NotPermissionException();
                //}

                backing.Backing.BackedDate = CommonUtils.ConvertDateTimeFromUtc(backing.Backing.BackedDate);

                var projectBackDto = new ProjectBackDTO
                {
                    BackedDate = backing.Backing.BackedDate,
                    ProjectCode = backing.Backing.Project.ProjectCode,
                    RewardPkgID = backing.RewardPkgID,
                    Description = backing.Description,
                    PledgeAmount = backing.PledgedAmount,
                    Email = backing.Email,
                    Quantity = backing.Quantity,
                    Address = backing.Address,
                    BackerName = backing.BackerName,
                    PhoneNumber = backing.PhoneNumber,
                    ProjectName = backing.Backing.Project.Title,
                    ProjectOwner = backing.Backing.Project.Creator.UserInfo.FullName,
                    RewardPkgDesc = backing.RewardPkg.Description,
                    RewardPkgType = backing.RewardPkg.Type,
                    BackerImg = backing.Backing.User.UserInfo.ProfileImage,
                    BackerUsername = backing.Backing.User.Username,
                    ProjectOwnerUsername = backing.Backing.Project.Creator.Username,
                    TransactionId = backing.TransactionId,
                    OrderId = backing.OrderId
                };

                return projectBackDto;
            }
        }
        #region Admin

        /// <summary>
        /// Get backing detail for admin
        /// </summary>
        /// <param name="backingId"></param>
        /// <returns>projectBackDTO</returns>
        public ProjectBackDTO AdminGetBackingDetail(int backingId)
        {
            using (var db = new DDLDataContext())
            {
                var backing = db.Backings.SingleOrDefault(x => x.BackingID == backingId);

                if (backing == null)
                {
                    throw new KeyNotFoundException();
                }

                backing.BackedDate = CommonUtils.ConvertDateTimeFromUtc(backing.BackedDate);

                var projectBackDto = new ProjectBackDTO
                {
                    BackedDate = backing.BackedDate,
                    ProjectCode = backing.Project.ProjectCode,
                    RewardPkgID = backing.BackingDetail.RewardPkgID,
                    Description = backing.BackingDetail.Description,
                    PledgeAmount = backing.BackingDetail.PledgedAmount,
                    Email = backing.BackingDetail.Email,
                    Quantity = backing.BackingDetail.Quantity,
                    Address = backing.BackingDetail.Address,
                    BackerName = backing.BackingDetail.BackerName,
                    PhoneNumber = backing.BackingDetail.PhoneNumber,
                    ProjectName = backing.Project.Title,
                    ProjectOwner = backing.Project.Creator.UserInfo.FullName,
                    RewardPkgDesc = backing.BackingDetail.RewardPkg.Description,
                    RewardPkgType = backing.BackingDetail.RewardPkg.Type,
                    BackerImg = backing.User.UserInfo.ProfileImage,
                    BackerUsername = backing.User.Username,
                    ProjectOwnerUsername = backing.Project.Creator.Username,
                    TransactionId = backing.BackingDetail.TransactionId,
                    OrderId = backing.BackingDetail.OrderId
                };

                return projectBackDto;
            }
        }

        /// <summary>
        /// Get basic statistic of project for admin
        /// </summary>
        /// <returns>AdminProjectInfoDTO</returns>
        public AdminProjectGeneralInfoDTO AdminProjectGeneralInfo()
        {
            using (var db = new DDLDataContext())
            {
                var pending = db.Projects.Count(x => x.Status == DDLConstants.ProjectStatus.PENDING);
                var approved = db.Projects.Count(x => x.Status == DDLConstants.ProjectStatus.APPROVED);
                var suspended = db.Projects.Count(x => x.Status == DDLConstants.ProjectStatus.SUSPENDED);
                var funded = db.Projects.Count(x => x.IsFunded == true);
                var total = db.Projects.Count(x => x.Status != DDLConstants.ProjectStatus.DRAFT);
                var expired = db.Projects.Count(x => x.Status == DDLConstants.ProjectStatus.EXPIRED);

                var AdminProjectInfoDTO = new AdminProjectGeneralInfoDTO
                {
                    ApprovedProject = approved,
                    ExpriredProject = expired,
                    SucceedProject = funded,
                    PendingProject = pending,
                    SuspendedProject = suspended,
                    TotalProject = total
                };

                return AdminProjectInfoDTO;
            }
        }

        /// <summary>
        /// Get pending project list
        /// </summary>
        /// <returns>pendingList</returns>
        public List<ProjectBasicListDTO> GetPendingProjectList()
        {
            using (var db = new DDLDataContext())
            {
                // Get rewardPkg list
                var pendingList = (from Project in db.Projects
                                   where Project.Status == DDLConstants.ProjectStatus.PENDING || Project.Status == DDLConstants.ProjectStatus.REJECTED
                                   orderby Project.CreatedDate descending
                                   select new ProjectBasicListDTO
                                   {
                                       ProjectCode = Project.ProjectCode,
                                       ProjectID = Project.ProjectID,
                                       Title = Project.Title,
                                       Category = Project.Category.Name,
                                       CreatorEmail = Project.Creator.Email,
                                       FundingGoal = Project.FundingGoal,
                                       ExpireDate = Project.ExpireDate,
                                       Status = Project.Status,
                                       CurrentFunded = Project.CurrentFunded,
                                       CreatorUsername = Project.Creator.Username,
                                       CreatorFullname = Project.Creator.UserInfo.FullName
                                   }).ToList();

                pendingList.ForEach(x => x.ExpireDate = CommonUtils.ConvertDateTimeFromUtc(x.ExpireDate.GetValueOrDefault()));

                return pendingList;
            }
        }

        /// <summary>
        /// Get project list
        /// </summary>
        /// <returns></returns>
        public List<ProjectBasicListDTO> GetProjectList()
        {
            using (var db = new DDLDataContext())
            {
                // Get rewardPkg list
                var projectList = (from Project in db.Projects
                                   where Project.Status != DDLConstants.ProjectStatus.DRAFT
                                   orderby Project.CreatedDate ascending
                                   select new ProjectBasicListDTO
                                   {
                                       ProjectCode = Project.ProjectCode,
                                       ProjectID = Project.ProjectID,
                                       Title = Project.Title,
                                       Category = Project.Category.Name,
                                       CreatorEmail = Project.Creator.Email,
                                       FundingGoal = Project.FundingGoal,
                                       ExpireDate = Project.ExpireDate,
                                       Status = Project.Status,
                                       CurrentFunded = Project.CurrentFunded,
                                       TotalBacking = Project.Backings.Count,
                                       CreatorUsername = Project.Creator.Username,
                                       CreatorFullname = Project.Creator.UserInfo.FullName,
                                       IsExprired = Project.IsExprired,
                                       IsFunded = Project.IsFunded
                                   }).ToList();

                projectList.ForEach(x => x.ExpireDate = CommonUtils.ConvertDateTimeFromUtc(x.ExpireDate.GetValueOrDefault()));

                return projectList;
            }
        }

        /// <summary>
        /// Change project status by admin
        /// </summary>
        /// <param name="project"></param>
        /// <returns>bool</returns>
        public bool AdminChangeProjectStatus(ProjectEditDTO project, string senderName)
        {
            using (var db = new DDLDataContext())
            {
                var updateProject = db.Projects.SingleOrDefault(x => x.ProjectID == project.ProjectID);

                if (updateProject == null)
                {
                    throw new KeyNotFoundException();
                }

                updateProject.Status = project.Status;


                var projectOwner = db.DDL_Users.SingleOrDefault(x => x.DDL_UserID == updateProject.CreatorID);

                if (projectOwner == null)
                {
                    throw new KeyNotFoundException();
                }

                string type = string.Empty;
                if (updateProject.Status == DDLConstants.ProjectStatus.SUSPENDED)
                {
                    type = "ngừng";
                }
                if (updateProject.Status == DDLConstants.ProjectStatus.APPROVED)
                {
                    type = "duyệt thành công";
                }
                if (updateProject.Status == DDLConstants.ProjectStatus.REJECTED)
                {
                    type = "từ chối";
                }

                db.SaveChanges();

                // Send email to project owner
                MailHelper.Instance.SendMailChangeProjectStatus(projectOwner.Email, projectOwner.UserInfo.FullName, type, updateProject.Title);

                // Send message to project owner
                var message = new NewMessageDTO
                {
                    Title = "Dandelion - " + type + " dự án " + updateProject.Title + "!",
                    ToUser = projectOwner.Username,
                    Content = "Xin chào " + projectOwner.UserInfo.FullName + "," +
                            "<br/>Chúng tôi vừa " + type + " dự án " + updateProject.Title + " của bạn." +
                            "<br/>Để biết thêm chi tiết xin liện hệ với admin qua email" +
                            "<br/>dandelion.system@gmail.com"
                };
                MessageRepository.Instance.CreateNewConversation(message, senderName);

                return true;
            }
        }

        /// <summary>
        /// Get general information for admin dashboard
        /// </summary>
        /// <returns>AdminDashboardInfoDTO</returns>
        public AdminDashboardInfoDTO AdminDashboardInfo()
        {
            using (var db = new DDLDataContext())
            {
                // Get all projects
                var projects = db.Projects.Where(x => x.Status != DDLConstants.ProjectStatus.DRAFT).ToList();

                // Count user
                var totalUser = db.DDL_Users.Count();
                // Count project
                var totalProject = projects.Count();

                // Caculate total funded
                var pledge = db.BackingDetails.GroupBy(o => o.BackingID)
                   .Select(g => new { BackingID = g.Key, total = g.Sum(i => i.PledgedAmount) });
                decimal totalFunded = 0;

                foreach (var group in pledge)
                {
                    totalFunded += group.total;
                }

                // Caculate total profit
                var succeedProject = (from Project in projects
                                      where Project.IsFunded && Project.IsExprired
                                      select Project).ToList();

                decimal totalProfit = 0;

                foreach (var project in succeedProject)
                {
                    if (project.FundingGoal <= 50000000)
                    {
                        totalProfit += 5 * project.CurrentFunded / 100;
                    }
                    else if (project.FundingGoal <= 100000000 && project.FundingGoal > 50000000)
                    {
                        totalProfit += 4 * project.CurrentFunded / 100;
                    }
                    else if (project.FundingGoal <= 500000000 && project.FundingGoal > 100000000)
                    {
                        totalProfit += 3 * project.CurrentFunded / 100;
                    }
                    else
                    {
                        totalProfit += 2 * project.CurrentFunded / 100;
                    }
                }

                // Count pending project
                var pending = projects.Count(x => x.Status == DDLConstants.ProjectStatus.PENDING);
                // Count live project
                var approved = projects.Count(x => x.Status == DDLConstants.ProjectStatus.APPROVED && x.IsExprired);
                // Count succeed project
                var funed = projects.Count(x => x.IsFunded);
                // Count rank A project
                var rankA = projects.Count(x => x.FundingGoal > 500000000);
                // Count rank B project
                var rankB = projects.Count(x => x.FundingGoal <= 500000000 && x.FundingGoal > 100000000);
                // Count rank C project
                var rankC = projects.Count(x => x.FundingGoal <= 100000000 && x.FundingGoal > 50000000);
                // Count rank D project
                var rankD = projects.Count(x => x.FundingGoal <= 50000000);
                // Count fail project
                var failProject = projects.Count(x => x.IsFunded == false && x.IsExprired);

                var AdminDashboardInfoDTO = new AdminDashboardInfoDTO
                {
                    TotalProject = totalProject,
                    LiveProject = approved,
                    NewProject = pending,
                    TotalFund = totalFunded,
                    TotalProfit = totalProfit,
                    TotalUser = totalUser,
                    TotalSucceed = funed,
                    RankA = rankA,
                    RankB = rankB,
                    RankC = rankC,
                    RankD = rankD,
                    TotalFail = failProject
                };

                return AdminDashboardInfoDTO;
            }
        }

        /// <summary>
        /// Get 5 top projects
        /// </summary>
        /// <returns></returns>
        public List<ProjectBasicListDTO> AdminGetTopProjectList()
        {
            using (var db = new DDLDataContext())
            {
                // Get rewardPkg list
                var projectList = (from Project in db.Projects
                                   where Project.IsExprired && Project.IsFunded
                                   select new ProjectBasicListDTO
                                   {
                                       ProjectCode = Project.ProjectCode,
                                       Title = Project.Title,
                                       Category = Project.Category.Name,
                                       CreatorEmail = Project.Creator.Email,
                                       FundingGoal = Project.FundingGoal,
                                       Status = Project.Status,
                                       CurrentFunded = Project.CurrentFunded,
                                       CreatorFullname = Project.Creator.UserInfo.FullName,
                                       CreatorUsername = Project.Creator.Username
                                   }).ToList();

                projectList = projectList.OrderByDescending(x => x.CurrentFunded).Take(5).ToList();

                return projectList;
            }
        }


        /// <summary>
        /// Get project statistic every month in year
        /// </summary>
        /// <returns></returns>
        public AdminProjectStatisticDTO AdminProjectStatistic(int year)
        {
            using (var db = new DDLDataContext())
            {
                var projects = db.Projects.ToList();

                foreach (var project in projects)
                {
                    project.CreatedDate = CommonUtils.ConvertDateTimeFromUtc(project.CreatedDate);
                    project.ExpireDate = CommonUtils.ConvertDateTimeFromUtc(project.ExpireDate.GetValueOrDefault());
                }

                // Caculate created project
                var createdProject = projects.Where(x => x.CreatedDate.Year == year)
                  .GroupBy(x => new { x.CreatedDate.Month })
                  .Select(grp => new
                  {
                      Month = grp.Key.Month,
                      Total = grp.Count()
                  });
                var listCreated = new List<Statistic>();
                foreach (var project in createdProject)
                {
                    var sum = new Statistic
                    {
                        Amount = project.Total,
                        Month = project.Month
                    };
                    listCreated.Add(sum);
                }

                // Caculate Success project
                var successProject = projects.Where(x => x.ExpireDate.Value.Year == year && x.IsFunded && x.IsExprired)
                  .GroupBy(x => new { x.ExpireDate.Value.Month })
                  .Select(grp => new
                  {
                      Month = grp.Key.Month,
                      Total = grp.Count(),
                  });
                var listSuccess = new List<Statistic>();
                foreach (var project in successProject)
                {
                    var sum = new Statistic
                    {
                        Amount = project.Total,
                        Month = project.Month
                    };
                    listSuccess.Add(sum);
                }

                // Caculate profit
                var succeedProject = (from Project in projects
                                      where Project.IsFunded && Project.IsExprired && Project.ExpireDate.Value.Year == year
                                      select Project).ToList();
                decimal profit = 0;
                List<KeyValuePair<int, decimal>> singleProject = new List<KeyValuePair<int, decimal>>();
                foreach (var project in succeedProject)
                {
                    if (project.FundingGoal <= 50000000)
                    {
                        profit += 5 * project.CurrentFunded / 100;
                    }
                    else if (project.FundingGoal <= 100000000 && project.FundingGoal > 50000000)
                    {
                        profit += 4 * project.CurrentFunded / 100;
                    }
                    else if (project.FundingGoal <= 500000000 && project.FundingGoal > 100000000)
                    {
                        profit += 3 * project.CurrentFunded / 100;
                    }
                    else
                    {
                        profit += 2 * project.CurrentFunded / 100;
                    }

                    singleProject.Add(new KeyValuePair<int, decimal>(project.ExpireDate.Value.Month, profit));

                    profit = 0;
                }
                var result = singleProject.GroupBy(r => r.Key).Select(r => new KeyValuePair<int, decimal>(r.Key, r.Sum(p => p.Value))).ToList();
                var listProfit = new List<Statistic>();
                foreach (var r in result)
                {
                    var sum = new Statistic
                    {
                        Amount = r.Value,
                        Month = r.Key,
                    };
                    listProfit.Add(sum);
                }


                // Caculate fail project
                var failProject = projects.Where(x => x.ExpireDate.Value.Year == year && x.IsFunded == false && x.IsExprired)
                  .GroupBy(x => new { x.ExpireDate.Value.Month })
                  .Select(grp => new
                  {
                      Month = grp.Key.Month,
                      Total = grp.Count()
                  });
                var listFail = new List<Statistic>();
                foreach (var project in failProject)
                {
                    var sum = new Statistic
                    {
                        Amount = project.Total,
                        Month = project.Month
                    };
                    listFail.Add(sum);
                }

                // Caculate total funded
                var pledge = db.Backings.Where(x => x.BackedDate.Year == year)
                    .GroupBy(x => new { x.BackedDate.Month })
                   .Select(g => new
                   {
                       Month = g.Key.Month,
                       Total = g.Sum(i => i.BackingDetail.PledgedAmount)
                   });
                var listFunded = new List<Statistic>();
                foreach (var amount in pledge)
                {
                    var sum = new Statistic
                    {
                        Amount = amount.Total,
                        Month = amount.Month
                    };
                    listFunded.Add(sum);
                }

                var list = new AdminProjectStatisticDTO
                {
                    Created = listCreated,
                    Succeed = listSuccess,
                    Fail = listFail,
                    Funded = listFunded,
                    Profit = listProfit
                };

                return list;
            }
        }

        /// <summary>
        /// Get project statistic table
        /// </summary>
        /// <returns>List<AdminDashboardInfoDTO></returns>
        public List<AdminDashboardInfoDTO> AdminStatisticTable(string option)
        {
            using (var db = new DDLDataContext())
            {
                var projects = db.Projects.ToList();

                foreach (var project in projects)
                {
                    project.CreatedDate = CommonUtils.ConvertDateTimeFromUtc(project.CreatedDate);
                    project.ExpireDate = CommonUtils.ConvertDateTimeFromUtc(project.ExpireDate.GetValueOrDefault());
                }

                var thisTime = DateTime.UtcNow;
                thisTime = CommonUtils.ConvertDateTimeFromUtc(thisTime);

                var listStatisticDTO = new List<AdminDashboardInfoDTO>();

                if (option == "year")
                {
                    for (int i = 2015; i <= thisTime.Year; i++)
                    {
                        // Count project
                        var totalProject =
                            projects.Count(x => x.Status != DDLConstants.ProjectStatus.DRAFT && x.CreatedDate.Year == i);

                        // Caculate total funded
                        var pledge = db.BackingDetails.Where(x => x.Backing.BackedDate.Year == i)
                            .GroupBy(o => o.BackingID)
                            .Select(g => new { BackingID = g.Key, total = g.Sum(j => j.PledgedAmount) });
                        decimal totalFunded = 0;

                        foreach (var group in pledge)
                        {
                            totalFunded += group.total;
                        }

                        // Caculate total profit
                        var succeedProject = (from Project in projects
                                              where Project.IsFunded && Project.IsExprired && Project.ExpireDate.Value.Year == i
                                              select Project).ToList();

                        decimal totalProfit = 0;
                        decimal succeedFunded = 0;

                        foreach (var project in succeedProject)
                        {
                            if (project.FundingGoal <= 50000000)
                            {
                                totalProfit += 5 * project.CurrentFunded / 100;
                            }
                            else if (project.FundingGoal <= 100000000 && project.FundingGoal > 50000000)
                            {
                                totalProfit += 4 * project.CurrentFunded / 100;
                            }
                            else if (project.FundingGoal <= 500000000 && project.FundingGoal > 100000000)
                            {
                                totalProfit += 3 * project.CurrentFunded / 100;
                            }
                            else
                            {
                                totalProfit += 2 * project.CurrentFunded / 100;
                            }

                            succeedFunded += project.CurrentFunded;
                        }

                        // Count live project
                        var approved = 0;
                        if (i == thisTime.Year)
                        {
                            approved = projects.Count(
                                    x => x.Status == DDLConstants.ProjectStatus.APPROVED && x.IsExprired == false);
                        }
                        // Count succeed project
                        var funed = db.Projects.Count(x => x.IsFunded && x.IsExprired && x.ExpireDate.Value.Year == i);
                        // Count fail project
                        var failProject = db.Projects.Count(x => x.IsFunded == false && x.IsExprired && x.ExpireDate.Value.Year == i);

                        var adminDashboardInfoDTO = new AdminDashboardInfoDTO
                        {
                            TotalProject = totalProject,
                            TotalFund = totalFunded,
                            SucceedFund = succeedFunded,
                            TotalProfit = totalProfit,
                            TotalSucceed = funed,
                            TotalFail = failProject,
                            Time = i.ToString(),
                            LiveProject = approved
                        };

                        listStatisticDTO.Add(adminDashboardInfoDTO);
                    }
                }
                else
                {
                    for (int i = 2015; i <= thisTime.Year; i++)
                    {
                        for (int j = 1; j < 13; j++)
                        {
                            // Count project
                            var totalProject =
                                projects.Count(x => x.Status != DDLConstants.ProjectStatus.DRAFT && x.CreatedDate.Year == i && x.CreatedDate.Month == j);

                            // Caculate total funded
                            var pledge = db.BackingDetails.Where(x => x.Backing.BackedDate.Year == i && x.Backing.BackedDate.Month == j)
                                .GroupBy(o => o.BackingID)
                                .Select(g => new { BackingID = g.Key, total = g.Sum(c => c.PledgedAmount) });
                            decimal totalFunded = 0;

                            foreach (var group in pledge)
                            {
                                totalFunded += group.total;
                            }

                            // Caculate total profit
                            var succeedProject = (from Project in projects
                                                  where Project.IsFunded && Project.IsExprired && Project.ExpireDate.Value.Year == i && Project.ExpireDate.Value.Month == j
                                                  select Project).ToList();

                            decimal totalProfit = 0;
                            decimal succeedFunded = 0;

                            foreach (var project in succeedProject)
                            {
                                if (project.FundingGoal <= 50000000)
                                {
                                    totalProfit += 5 * project.CurrentFunded / 100;
                                }
                                else if (project.FundingGoal <= 100000000 && project.FundingGoal > 50000000)
                                {
                                    totalProfit += 4 * project.CurrentFunded / 100;
                                }
                                else if (project.FundingGoal <= 500000000 && project.FundingGoal > 100000000)
                                {
                                    totalProfit += 3 * project.CurrentFunded / 100;
                                }
                                else
                                {
                                    totalProfit += 2 * project.CurrentFunded / 100;
                                }

                                succeedFunded += project.CurrentFunded;
                            }

                            // Count live project
                            var approved = 0;
                            if (i == thisTime.Year && j == thisTime.Month)
                            {
                                approved = projects.Count(
                                        x => x.Status == DDLConstants.ProjectStatus.APPROVED && x.IsExprired == false);
                            }

                            // Count succeed project
                            var funed = db.Projects.Count(x => x.IsFunded && x.IsExprired && x.ExpireDate.Value.Year == i && x.ExpireDate.Value.Month == j);
                            // Count fail project
                            var failProject = db.Projects.Count(x => x.IsFunded == false && x.IsExprired && x.ExpireDate.Value.Month == j && x.ExpireDate.Value.Year == i);

                            var adminDashboardInfoDTO = new AdminDashboardInfoDTO
                            {
                                TotalProject = totalProject,
                                TotalFund = totalFunded,
                                SucceedFund = succeedFunded,
                                TotalProfit = totalProfit,
                                TotalSucceed = funed,
                                TotalFail = failProject,
                                Time = j.ToString() + "/" + i.ToString(),
                                LiveProject = approved
                            };

                            listStatisticDTO.Add(adminDashboardInfoDTO);

                            if (i == thisTime.Year && j == thisTime.Month) break;
                        }
                    }
                }

                return listStatisticDTO;
            }
        }

        /// <summary>
        /// Get project detail for admin
        /// </summary>
        /// <param name="projectCode"></param>
        /// <returns></returns>
        public ProjectDetailDTO AdminGetProjectDetail(string projectCode)
        {
            using (var db = new DDLDataContext())
            {
                if (string.IsNullOrEmpty(projectCode))
                {
                    throw new KeyNotFoundException();
                }
                // Create Project query from dB.
                var projectDetail = (from project in db.Projects
                                     where project.ProjectCode.Equals(projectCode.ToUpper())
                                     && (project.Status != DDLConstants.ProjectStatus.DRAFT)
                                     select new ProjectDetailDTO
                                     {
                                         CategoryID = project.CategoryID,
                                         CreatedDate = project.CreatedDate,
                                         Description = project.Description,
                                         Title = project.Title,
                                         ImageUrl = project.ImageUrl,
                                         Status = project.Status,
                                         SubDescription = project.SubDescription,
                                         ProjectCode = project.ProjectCode,
                                         CurrentFunded = project.CurrentFunded,
                                         ExpireDate = project.ExpireDate,
                                         Risk = project.Risk,
                                         FundingGoal = project.FundingGoal,
                                         Location = project.Location,
                                         IsExprired = project.IsExprired,
                                         VideoUrl = project.VideoUrl,
                                         CategoryName = project.Category.Name,
                                         ProjectID = project.ProjectID,
                                         NumberBacked = project.Backings.Count,
                                         //NumberComment =
                                         //    project.Creator.Username == userName
                                         //        ? project.Comments.Count
                                         //        : project.Comments.Count(x => !x.IsHide),
                                         NumberUpdate = project.UpdateLogs.Count,
                                         IsFunded = project.IsFunded,
                                         Creator = new CreatorDTO
                                         {
                                             FullName = project.Creator.UserInfo.FullName,
                                             UserName = project.Creator.Username,
                                             NumberBacked = project.Creator.Backings.Count,
                                             NumberCreated =
                                                 project.Creator.CreatedProjects.Count(x => x.Status != DDLConstants.ProjectStatus.DRAFT
                                                                                            &&
                                                                                            x.Status !=
                                                                                            DDLConstants.ProjectStatus.REJECTED
                                                                                            &&
                                                                                            x.Status !=
                                                                                            DDLConstants.ProjectStatus.PENDING),
                                             ProfileImage = project.Creator.UserInfo.ProfileImage
                                         },
                                     }).FirstOrDefault();


                // Check project exist?
                if (projectDetail == null)
                {
                    throw new KeyNotFoundException();
                }

                // Get rewards.
                var rewardDetail =
                    db.RewardPkgs.Where(x => x.ProjectID == projectDetail.ProjectID && !x.IsHide).ToList();
                var rewardDto = new List<RewardPkgDTO>();
                foreach (var reward in rewardDetail)
                {
                    var Reward = new RewardPkgDTO
                    {
                        Backers = reward.BackingDetails.Count(),
                        Description = reward.Description,
                        EstimatedDelivery =
                            CommonUtils.ConvertDateTimeFromUtc(reward.EstimatedDelivery.GetValueOrDefault()),
                        PledgeAmount = reward.PledgeAmount,
                        Quantity = reward.Quantity
                    };
                    rewardDto.Add(Reward);
                }
                projectDetail.RewardDetail = rewardDto;

                // Convert datetime to gmt+7
                projectDetail.CreatedDate = CommonUtils.ConvertDateTimeFromUtc(projectDetail.CreatedDate);
                projectDetail.ExpireDate =
                    CommonUtils.ConvertDateTimeFromUtc(projectDetail.ExpireDate.GetValueOrDefault());

                // Set number exprire day.
                var timespan = projectDetail.ExpireDate - CommonUtils.DateTodayGMT7();
                projectDetail.NumberDays = timespan.GetValueOrDefault().Days;
                if (projectDetail.NumberDays < 0)
                {
                    projectDetail.NumberDays = 0;
                }

                return projectDetail;
            }
        }
        #endregion


        #endregion

        #region HuyNM Question
        /// <summary>
        /// Get question list of a project
        /// </summary>
        /// <returns>questionList</returns>
        public List<QuestionDTO> GetQuestion(int ProjectID)
        {
            using (var db = new DDLDataContext())
            {
                // Get rewardPkg list
                var questionList = (from Question in db.Questions
                                    where Question.ProjectID == ProjectID
                                    orderby Question.CreatedDate ascending
                                    select new QuestionDTO
                                    {
                                        Answer = Question.Answer,
                                        CreatedDate = Question.CreatedDate,
                                        QuestionContent = Question.QuestionContent,
                                        QuestionID = Question.QuestionID
                                    }).ToList();

                questionList.ForEach(x => x.CreatedDate = CommonUtils.ConvertDateTimeFromUtc(x.CreatedDate));

                return questionList;
            }
        }

        /// <summary>
        /// Create a new QA
        /// </summary>
        /// <param name="ProjectID"></param>
        /// <param name="question"></param>
        /// <returns>newQuestionDTO</returns>
        public QuestionDTO CreateQuestion(int ProjectID, QuestionDTO question, string username)
        {
            using (var db = new DDLDataContext())
            {
                var project = db.Projects.SingleOrDefault(x => x.ProjectID == ProjectID);
                if (project == null)
                {
                    throw new KeyNotFoundException();
                }

                if (project.Creator.Username != username)
                {
                    throw new NotPermissionException();
                }

                var newQuestion = db.Questions.Create();
                newQuestion.ProjectID = ProjectID;
                newQuestion.CreatedDate = DateTime.UtcNow;
                newQuestion.QuestionContent = question.QuestionContent;
                newQuestion.Answer = question.Answer;

                project.UpdatedDate = DateTime.UtcNow;

                db.Questions.Add(newQuestion);
                db.SaveChanges();

                var newQuestionDTO = new QuestionDTO
                {
                    Answer = newQuestion.Answer,
                    CreatedDate = newQuestion.CreatedDate,
                    QuestionContent = newQuestion.QuestionContent,
                    QuestionID = newQuestion.QuestionID
                };

                newQuestionDTO.CreatedDate = CommonUtils.ConvertDateTimeFromUtc(newQuestionDTO.CreatedDate);

                return newQuestionDTO;
            }
        }

        /// <summary>
        /// Edit QAs
        /// </summary>
        /// <returns>boolean</returns>
        public bool EditQuestion(List<QuestionDTO> question, string username)
        {
            using (var db = new DDLDataContext())
            {
                foreach (var qa in question)
                {
                    var updateQuestion = db.Questions.SingleOrDefault(x => x.QuestionID == qa.QuestionID);

                    if (updateQuestion == null)
                    {
                        throw new KeyNotFoundException();
                    }

                    if (updateQuestion.Project.Creator.Username != username)
                    {
                        throw new NotPermissionException();
                    }

                    updateQuestion.Answer = qa.Answer;
                    updateQuestion.CreatedDate = DateTime.UtcNow;
                    updateQuestion.QuestionContent = qa.QuestionContent;

                    updateQuestion.Project.UpdatedDate = DateTime.UtcNow;

                    db.SaveChanges();
                }

                return true;
            }
        }

        /// <summary>
        /// Edit single question
        /// </summary>
        /// <param name="question"></param>
        /// <returns>updateLogDTO</returns>
        public QuestionDTO EditSingleQuestion(QuestionDTO question, string username)
        {
            using (var db = new DDLDataContext())
            {
                var updateQuestion = db.Questions.SingleOrDefault(x => x.QuestionID == question.QuestionID);

                if (updateQuestion == null)
                {
                    throw new KeyNotFoundException();
                }

                if (updateQuestion.Project.Creator.Username != username)
                {
                    throw new NotPermissionException();
                }

                updateQuestion.Answer = question.Answer;
                updateQuestion.CreatedDate = DateTime.UtcNow;
                updateQuestion.QuestionContent = question.QuestionContent;

                updateQuestion.Project.UpdatedDate = DateTime.UtcNow;

                db.SaveChanges();

                var questionDTO = new QuestionDTO
                {
                    Answer = updateQuestion.Answer,
                    CreatedDate = updateQuestion.CreatedDate,
                    QuestionContent = updateQuestion.QuestionContent,
                    QuestionID = updateQuestion.QuestionID,
                };

                questionDTO.CreatedDate = CommonUtils.ConvertDateTimeFromUtc(questionDTO.CreatedDate);

                return questionDTO;
            }
        }

        /// <summary>
        /// Delete a QA
        /// </summary>
        /// <param name="questionID"></param>
        /// <returns>boolean</returns>
        public bool DeleteQuestion(int questionID, string username)
        {
            using (var db = new DDLDataContext())
            {
                var deleteQuestion = db.Questions.SingleOrDefault(x => x.QuestionID == questionID);

                if (deleteQuestion == null)
                {
                    throw new KeyNotFoundException();
                }

                if (deleteQuestion.Project.Creator.Username != username)
                {
                    throw new NotPermissionException();
                }

                deleteQuestion.Project.UpdatedDate = DateTime.UtcNow;

                db.Questions.Remove(deleteQuestion);
                db.SaveChanges();

                return true;
            }
        }
        #endregion

        #region HuyNM Reward
        /// <summary>
        /// Get rewardPkg of a project by project id
        /// </summary>
        /// <returns>rewardList</returns>
        public List<RewardPkgDTO> GetRewardPkg(int ProjectID)
        {
            using (var db = new DDLDataContext())
            {
                // Get rewardPkg list
                var rewardList = (from RewardPkg in db.RewardPkgs
                                  where RewardPkg.ProjectID == ProjectID
                                  orderby RewardPkg.PledgeAmount ascending
                                  select new RewardPkgDTO()
                                  {
                                      Description = RewardPkg.Description,
                                      PledgeAmount = RewardPkg.PledgeAmount,
                                      EstimatedDelivery = RewardPkg.EstimatedDelivery,
                                      IsHide = RewardPkg.IsHide,
                                      Quantity = RewardPkg.Quantity,
                                      RewardPkgID = RewardPkg.RewardPkgID,
                                      Type = RewardPkg.Type,
                                      CurrentQuantity = RewardPkg.CurrentQuantity,
                                      Backers = db.BackingDetails.Count(t => t.RewardPkgID == RewardPkg.RewardPkgID)
                                  }).ToList();

                rewardList.ForEach(x => x.EstimatedDelivery = CommonUtils.ConvertDateTimeFromUtc(x.EstimatedDelivery.GetValueOrDefault()));

                return rewardList;
            }
        }

        /// <summary>
        /// Get rewardPkg of a project by project code
        /// </summary>
        /// <returns>rewardList</returns>
        public List<RewardPkgDTO> GetRewardPkgByCode(string code)
        {
            using (var db = new DDLDataContext())
            {
                var project = db.Projects.SingleOrDefault(x => x.ProjectCode == code);

                // Get rewardPkg list
                var rewardList = (from RewardPkg in db.RewardPkgs
                                  where RewardPkg.ProjectID == project.ProjectID && RewardPkg.IsHide == false
                                  orderby RewardPkg.PledgeAmount ascending
                                  select new RewardPkgDTO()
                                  {
                                      Description = RewardPkg.Description,
                                      PledgeAmount = RewardPkg.PledgeAmount,
                                      EstimatedDelivery = RewardPkg.EstimatedDelivery,
                                      IsHide = RewardPkg.IsHide,
                                      Quantity = RewardPkg.Quantity,
                                      RewardPkgID = RewardPkg.RewardPkgID,
                                      Type = RewardPkg.Type,
                                      CurrentQuantity = RewardPkg.CurrentQuantity,
                                      Backers = db.BackingDetails.Count(t => t.RewardPkgID == RewardPkg.RewardPkgID)
                                  }).ToList();

                rewardList.ForEach(x => x.EstimatedDelivery = CommonUtils.ConvertDateTimeFromUtc(x.EstimatedDelivery.GetValueOrDefault()));

                return rewardList.ToList();
            }
        }

        /// <summary>
        /// Create a new rewardPkg
        /// </summary>
        /// <param name="ProjectID"></param>
        /// <param name="rewardPkg"></param>
        /// <returns>newRewardPkg</returns>
        public RewardPkgDTO CreateRewardPkg(int ProjectID, RewardPkgDTO rewardPkg, string username)
        {
            using (var db = new DDLDataContext())
            {
                var project = db.Projects.SingleOrDefault(x => x.ProjectID == ProjectID);

                if (project == null)
                {
                    throw new KeyNotFoundException();
                }

                if (project.Creator.Username != username)
                {
                    throw new NotPermissionException();
                }

                var newRewardPkg = db.RewardPkgs.Create();

                newRewardPkg.ProjectID = ProjectID;
                newRewardPkg.PledgeAmount = rewardPkg.PledgeAmount;
                newRewardPkg.Description = rewardPkg.Description;
                newRewardPkg.EstimatedDelivery = rewardPkg.EstimatedDelivery;
                newRewardPkg.Quantity = rewardPkg.Quantity;
                newRewardPkg.Type = rewardPkg.Type;
                newRewardPkg.IsHide = rewardPkg.IsHide;

                if (newRewardPkg.EstimatedDelivery != null)
                {
                    newRewardPkg.EstimatedDelivery =
                        CommonUtils.ConvertDateTimeToUtc(newRewardPkg.EstimatedDelivery.GetValueOrDefault());
                }

                project.UpdatedDate = DateTime.UtcNow;

                db.RewardPkgs.Add(newRewardPkg);
                db.SaveChanges();

                var newRewardPkgDTO = new RewardPkgDTO
                {
                    Backers = db.BackingDetails.Count(t => t.RewardPkgID == newRewardPkg.RewardPkgID),
                    PledgeAmount = newRewardPkg.PledgeAmount,
                    Description = newRewardPkg.Description,
                    EstimatedDelivery = newRewardPkg.EstimatedDelivery,
                    Quantity = newRewardPkg.Quantity,
                    Type = newRewardPkg.Type,
                    IsHide = newRewardPkg.IsHide,
                    RewardPkgID = newRewardPkg.RewardPkgID
                };

                if (newRewardPkgDTO.EstimatedDelivery != null)
                {
                    newRewardPkgDTO.EstimatedDelivery = CommonUtils.ConvertDateTimeFromUtc(newRewardPkgDTO.EstimatedDelivery.GetValueOrDefault());
                }

                return newRewardPkgDTO;
            }
        }

        /// <summary>
        /// Edit rewardPkgs
        /// </summary>
        /// <returns></returns>
        public bool EditRewardPkg(RewardPkgDTO rewardPkg, string username)
        {
            using (var db = new DDLDataContext())
            {
                var updateReward = db.RewardPkgs.SingleOrDefault(x => x.RewardPkgID == rewardPkg.RewardPkgID);

                if (updateReward == null)
                {
                    throw new KeyNotFoundException();
                }

                if (updateReward.Project.Creator.Username != username)
                {
                    throw new NotPermissionException();
                }

                if (updateReward.EstimatedDelivery != null)
                {
                    updateReward.EstimatedDelivery = CommonUtils.ConvertDateTimeToUtc(updateReward.EstimatedDelivery.GetValueOrDefault());
                }

                updateReward.Description = rewardPkg.Description;
                updateReward.EstimatedDelivery = rewardPkg.EstimatedDelivery;
                updateReward.Quantity = rewardPkg.Quantity;
                updateReward.IsHide = rewardPkg.IsHide;
                updateReward.Type = rewardPkg.Type;
                updateReward.PledgeAmount = rewardPkg.PledgeAmount;

                updateReward.Project.UpdatedDate = DateTime.UtcNow;

                db.SaveChanges();

                return true;
            }
        }

        /// <summary>
        /// Delete a rewardPkg
        /// </summary>
        /// <param name="rewardPkgID"></param>
        /// <returns>boolean</returns>
        public bool DeleteRewardPkg(int rewardPkgID, string username)
        {
            using (var db = new DDLDataContext())
            {
                var deleteRewardPkg = db.RewardPkgs.SingleOrDefault(x => x.RewardPkgID == rewardPkgID);

                if (deleteRewardPkg == null)
                {
                    throw new KeyNotFoundException();
                }

                if (deleteRewardPkg.Project.Creator.Username != username)
                {
                    throw new NotPermissionException();
                }

                deleteRewardPkg.Project.UpdatedDate = DateTime.UtcNow;

                db.RewardPkgs.Remove(deleteRewardPkg);
                db.SaveChanges();

                return true;
            }
        }
        #endregion

        #region HuyNM UpdateLog
        /// <summary>
        /// Get UpdateLog of a project
        /// </summary>
        /// <returns>updateLogList</returns>
        public List<UpdateLogDTO> GetUpdateLog(int ProjectID)
        {
            using (var db = new DDLDataContext())
            {
                // Get updatelog list
                var updateLogList = (from UpdateLog in db.UpdateLogs
                                     where UpdateLog.ProjectID == ProjectID
                                     orderby UpdateLog.CreatedDate descending
                                     select new UpdateLogDTO()
                                     {
                                         Description = UpdateLog.Description,
                                         Title = UpdateLog.Title,
                                         CreatedDate = UpdateLog.CreatedDate,
                                         UpdateLogID = UpdateLog.UpdateLogID,
                                     }).ToList();

                updateLogList.ForEach(x => x.CreatedDate = CommonUtils.ConvertDateTimeFromUtc(x.CreatedDate));

                return updateLogList;
            }
        }

        /// <summary>
        /// Create a new updateLog
        /// </summary>
        /// <param name="ProjectID"></param>
        /// <param name="updateLog"></param>
        /// <returns>newRewardPkg</returns>
        public UpdateLogDTO CreateUpdateLog(int ProjectID, UpdateLogDTO newUpdateLog, string username)
        {
            using (var db = new DDLDataContext())
            {
                var project = db.Projects.SingleOrDefault(x => x.ProjectID == ProjectID);
                if (project == null)
                {
                    throw new KeyNotFoundException();
                }
                if (project.Creator.Username != username)
                {
                    throw new NotPermissionException();
                }

                var updateLog = db.UpdateLogs.Create();
                updateLog.ProjectID = ProjectID;
                updateLog.Description = newUpdateLog.Description;
                updateLog.CreatedDate = DateTime.UtcNow;
                updateLog.Title = newUpdateLog.Title;

                project.UpdatedDate = DateTime.UtcNow;

                db.UpdateLogs.Add(updateLog);
                db.SaveChanges();

                var updateLogDTO = new UpdateLogDTO
                {
                    Description = updateLog.Description,
                    Title = updateLog.Title,
                    CreatedDate = updateLog.CreatedDate,
                    UpdateLogID = updateLog.UpdateLogID
                };

                updateLogDTO.CreatedDate = CommonUtils.ConvertDateTimeFromUtc(updateLogDTO.CreatedDate);

                return updateLogDTO;
            }
        }

        /// <summary>
        /// Edit updateLog
        /// </summary>
        /// <returns>boolean</returns>
        public bool EditUpdateLog(List<UpdateLogDTO> updateLog, string username)
        {
            using (var db = new DDLDataContext())
            {
                foreach (var update in updateLog)
                {
                    var editLog = db.UpdateLogs.SingleOrDefault(x => x.UpdateLogID == update.UpdateLogID);

                    if (editLog == null)
                    {
                        throw new KeyNotFoundException();
                    }

                    if (editLog.Project.Creator.Username != username)
                    {
                        throw new NotPermissionException();
                    }

                    if (editLog.Description != update.Description || editLog.Title != update.Title)
                    {
                        editLog.Description = update.Description;
                        editLog.Title = update.Title;
                        editLog.CreatedDate = DateTime.UtcNow;
                        editLog.Project.UpdatedDate = DateTime.UtcNow;
                    }

                    db.SaveChanges();
                }

                return true;
            }
        }

        /// <summary>
        /// Edit single update log
        /// </summary>
        /// <param name="updateLog"></param>
        /// <returns>updateLogDTO</returns>
        public UpdateLogDTO EditSingleUpdateLog(UpdateLogDTO updateLog, string username)
        {
            using (var db = new DDLDataContext())
            {
                var editLog = db.UpdateLogs.SingleOrDefault(x => x.UpdateLogID == updateLog.UpdateLogID);

                if (editLog == null)
                {
                    throw new KeyNotFoundException();
                }

                if (editLog.Project.Creator.Username != username)
                {
                    throw new NotPermissionException();
                }

                editLog.Description = updateLog.Description;
                editLog.Title = updateLog.Title;
                editLog.CreatedDate = DateTime.UtcNow;
                editLog.Project.UpdatedDate = DateTime.UtcNow;

                db.SaveChanges();

                var updateLogDTO = new UpdateLogDTO
                {
                    Description = editLog.Description,
                    Title = editLog.Title,
                    CreatedDate = editLog.CreatedDate,
                    UpdateLogID = editLog.UpdateLogID,
                };

                updateLogDTO.CreatedDate = CommonUtils.ConvertDateTimeFromUtc(updateLogDTO.CreatedDate);

                return updateLogDTO;
            }
        }

        /// <summary>
        /// Delete a updateLog
        /// </summary>
        /// <param name="updateLogID"></param>
        /// <param name="username"></param>
        /// <returns>bool</returns>
        public bool DeleteUpdateLog(int updateLogID, string username)
        {
            using (var db = new DDLDataContext())
            {
                var deleteUpdateLog = db.UpdateLogs.SingleOrDefault(x => x.UpdateLogID == updateLogID);

                if (deleteUpdateLog == null)
                {
                    throw new KeyNotFoundException();
                }

                if (deleteUpdateLog.Project.Creator.Username != username)
                {
                    throw new NotPermissionException();
                }

                deleteUpdateLog.Project.UpdatedDate = DateTime.UtcNow;

                db.UpdateLogs.Remove(deleteUpdateLog);
                db.SaveChanges();

                return true;
            }
        }
        #endregion

        #region HuyNM Timeline
        /// <summary>
        /// Get timeline list of a project
        /// </summary>
        /// <returns>timelineList</returns>
        public List<TimeLineDTO> GetTimeLine(int ProjectID)
        {
            using (var db = new DDLDataContext())
            {
                // Get rewardPkg list
                var timelineList = (from Timeline in db.Timelines
                                    where Timeline.ProjectID == ProjectID
                                    orderby Timeline.DueDate ascending
                                    select new TimeLineDTO()
                                    {
                                        ImageUrl = Timeline.ImageUrl,
                                        DueDate = Timeline.DueDate,
                                        Description = Timeline.Description,
                                        Title = Timeline.Title,
                                        TimelineID = Timeline.TimelineID
                                    }).ToList();

                timelineList.ForEach(x => x.DueDate = CommonUtils.ConvertDateTimeFromUtc(x.DueDate));

                return timelineList;
            }
        }

        /// <summary>
        /// Create a new timeline point
        /// </summary>
        /// <param name="ProjectID"></param>
        /// <param name="timeline"></param>
        /// <returns>newRewardPkg</returns>
        public TimeLineDTO CreateTimeline(int id, TimeLineDTO timeline, string img)
        {
            using (var db = new DDLDataContext())
            {
                var project = db.Projects.SingleOrDefault(x => x.ProjectID == id);
                if (project == null)
                {
                    throw new KeyNotFoundException();
                }

                var newTimeline = db.Timelines.Create();
                newTimeline.ProjectID = project.ProjectID;
                newTimeline.Description = timeline.Description;
                newTimeline.DueDate = timeline.DueDate;
                newTimeline.ImageUrl = img;
                newTimeline.Title = timeline.Title;

                if (newTimeline.DueDate != null)
                {
                    newTimeline.DueDate = CommonUtils.ConvertDateTimeToUtc(newTimeline.DueDate);
                }

                db.Timelines.Add(newTimeline);
                db.SaveChanges();

                newTimeline.ImageUrl = newTimeline.ImageUrl + "_" + newTimeline.TimelineID;

                var newTimelineDTO = new TimeLineDTO
                {
                    Description = newTimeline.Description,
                    DueDate = newTimeline.DueDate,
                    ImageUrl = newTimeline.ImageUrl,
                    Title = newTimeline.Title,
                    TimelineID = newTimeline.TimelineID
                };

                newTimelineDTO.DueDate = CommonUtils.ConvertDateTimeFromUtc(newTimelineDTO.DueDate);

                return newTimelineDTO;
            }
        }

        /// <summary>
        /// Edit timeline
        /// </summary>
        /// <returns>boolean</returns>
        public bool EditTimeline(TimeLineDTO timeline, string img)
        {
            using (var db = new DDLDataContext())
            {
                var updateTimeline = db.Timelines.SingleOrDefault(x => x.TimelineID == timeline.TimelineID);

                if (updateTimeline == null)
                {
                    throw new KeyNotFoundException();
                }

                if (img != string.Empty)
                {
                    updateTimeline.ImageUrl = img;
                }

                updateTimeline.Description = timeline.Description;
                updateTimeline.DueDate = timeline.DueDate;
                updateTimeline.Title = timeline.Title;

                updateTimeline.DueDate = CommonUtils.ConvertDateTimeToUtc(updateTimeline.DueDate);

                db.SaveChanges();


                return true;
            }
        }

        /// <summary>
        /// Delete a timeline point
        /// </summary>
        /// <param name="timelineID"></param>
        /// <returns>boolean</returns>
        public bool DeleteTimeline(int timelineID)
        {
            using (var db = new DDLDataContext())
            {
                var deleteTimeline = db.Timelines.SingleOrDefault(x => x.TimelineID == timelineID);

                if (deleteTimeline == null)
                {
                    throw new KeyNotFoundException();
                }

                db.Timelines.Remove(deleteTimeline);
                db.SaveChanges();

                return true;
            }
        }
        #endregion

        public ProjectDetailDTO GetProjectByCode(string projectCode, string userName)
        {
            using (var db = new DDLDataContext())
            {
                if (string.IsNullOrEmpty(projectCode))
                {
                    throw new KeyNotFoundException();
                }
                var userCurrent = db.DDL_Users.FirstOrDefault(x => x.Username == userName);
                var temp = userCurrent != null ? userCurrent.Username : "";
                // Create Project query from dB.
                var projectDetail = (from project in db.Projects
                                     where project.ProjectCode.Equals(projectCode.ToUpper())
                                     && ((project.Status != DDLConstants.ProjectStatus.DRAFT && project.Status != DDLConstants.ProjectStatus.PENDING && project.Status != DDLConstants.ProjectStatus.REJECTED)
                                        || project.Creator.Username.Equals(temp, StringComparison.OrdinalIgnoreCase))
                                     select new ProjectDetailDTO
                                     {
                                         CategoryID = project.CategoryID,
                                         CreatedDate = project.CreatedDate,
                                         Description = project.Description,
                                         Title = project.Title,
                                         ImageUrl = project.ImageUrl,
                                         Status = project.Status,
                                         SubDescription = project.SubDescription,
                                         ProjectCode = project.ProjectCode,
                                         CurrentFunded = project.CurrentFunded,
                                         ExpireDate = project.ExpireDate,
                                         Risk = project.Risk,
                                         FundingGoal = project.FundingGoal,
                                         Location = project.Location,
                                         IsExprired = project.IsExprired,
                                         VideoUrl = project.VideoUrl,
                                         CategoryName = project.Category.Name,
                                         ProjectID = project.ProjectID,
                                         NumberBacked = project.Backings.Count,
                                         NumberComment =
                                             project.Creator.Username == userName
                                                 ? project.Comments.Count
                                                 : project.Comments.Count(x => !x.IsHide),
                                         NumberUpdate = project.UpdateLogs.Count,
                                         Creator = new CreatorDTO
                                         {
                                             FullName = project.Creator.UserInfo.FullName,
                                             UserName = project.Creator.Username,
                                             NumberBacked = project.Creator.Backings.Count,
                                             NumberCreated =
                                                 project.Creator.CreatedProjects.Count(x => x.Status != DDLConstants.ProjectStatus.DRAFT
                                                                                            &&
                                                                                            x.Status !=
                                                                                            DDLConstants.ProjectStatus.REJECTED
                                                                                            &&
                                                                                            x.Status !=
                                                                                            DDLConstants.ProjectStatus.PENDING),
                                             ProfileImage = project.Creator.UserInfo.ProfileImage
                                         },
                                     }).FirstOrDefault();


                // Check project exist?
                if (projectDetail == null)
                {
                    throw new KeyNotFoundException();
                }

                // Check current user remind.
                projectDetail.Reminded = userCurrent != null &&
                                         db.Reminds.Any(
                                             x =>
                                                 x.UserID == userCurrent.DDL_UserID &&
                                                 x.ProjectID == projectDetail.ProjectID);

                // Get rewards.
                //var rewardDetail =
                //    db.RewardPkgs.Where(x => x.ProjectID == projectDetail.ProjectID && !x.IsHide).ToList();
                var rewardDto = (from reward in db.RewardPkgs
                                 where reward.ProjectID == projectDetail.ProjectID && !reward.IsHide
                                 orderby reward.PledgeAmount
                                 select new RewardPkgDTO
                                 {
                                     Backers = reward.BackingDetails.Count,
                                     Description = reward.Description,
                                     EstimatedDelivery = reward.EstimatedDelivery,
                                     PledgeAmount = reward.PledgeAmount,
                                     Quantity = reward.Quantity,
                                     CurrentQuantity = reward.CurrentQuantity,
                                     Type = reward.Type
                                 }).ToList();

                rewardDto.ForEach(x => x.EstimatedDelivery = CommonUtils.ConvertDateTimeFromUtc(x.EstimatedDelivery.GetValueOrDefault()));

                //foreach (var reward in rewardDetail)
                //{
                //    var Reward = new RewardPkgDTO
                //    {
                //        Backers = reward.BackingDetails.Count(),
                //        Description = reward.Description,
                //        EstimatedDelivery =
                //            CommonUtils.ConvertDateTimeFromUtc(reward.EstimatedDelivery.GetValueOrDefault()),
                //        PledgeAmount = reward.PledgeAmount,
                //        Quantity = reward.Quantity
                //    };
                //    rewardDto.Add(Reward);
                //}
                projectDetail.RewardDetail = rewardDto;

                // Convert datetime to gmt+7
                projectDetail.CreatedDate = CommonUtils.ConvertDateTimeFromUtc(projectDetail.CreatedDate);
                projectDetail.ExpireDate =
                    CommonUtils.ConvertDateTimeFromUtc(projectDetail.ExpireDate.GetValueOrDefault());

                // Set number exprire day.
                var timespan = projectDetail.ExpireDate - CommonUtils.DateTodayGMT7();
                projectDetail.NumberDays = timespan.GetValueOrDefault().Days >= 0 ? timespan.GetValueOrDefault().Days : 0;

                return projectDetail;
            }
        }

        public List<CommentDTO> GetListComment(string projectCode, DateTime? lastDateTime, string userName)
        {
            using (var db = new DDLDataContext())
            {
                var project = db.Projects.FirstOrDefault(x => x.ProjectCode.Equals(projectCode, StringComparison.OrdinalIgnoreCase));
                if (project == null)
                {
                    throw new KeyNotFoundException();
                }
                // Get comments.
                var commentsList = (userName != null &&
                                    project.Creator.Username.Equals(userName, StringComparison.OrdinalIgnoreCase))
                    ? GetComments(project.ProjectID, true, lastDateTime)
                    : GetComments(project.ProjectID, false, lastDateTime);

                return commentsList;
            }
        }

        public List<UpdateLogDTO> GetListUpdateLog(string projectCode, string userName)
        {
            using (var db = new DDLDataContext())
            {
                var project = db.Projects.FirstOrDefault(x => x.ProjectCode == projectCode);
                if (project == null)
                {
                    throw new KeyNotFoundException();
                }

                // Get updateLog list.
                var updateLogsList = (from updateLog in db.UpdateLogs
                                      where updateLog.ProjectID == project.ProjectID
                                      orderby updateLog.CreatedDate descending
                                      select new UpdateLogDTO
                                      {
                                          CreatedDate = updateLog.CreatedDate,
                                          Description = updateLog.Description,
                                          Title = updateLog.Title,
                                          UpdateLogID = updateLog.UpdateLogID
                                      }).ToList();
                updateLogsList.ForEach(x => x.CreatedDate = CommonUtils.ConvertDateTimeFromUtc(x.CreatedDate));
                return updateLogsList;
            }
        }

        // 19/10/2015 - MaiCTP - Get BackedProjectHistory
        public List<ProjectBasicViewDTO> GetBackedProjectHistory(String userName)
        {

            using (var db = new DDLDataContext())
            {
                var Project = (from backing in db.Backings
                               from project in db.Projects
                               where backing.User.Username == userName && project.ProjectID == backing.ProjectID
                               select new ProjectBasicViewDTO
                                {
                                    ProjectID = project.ProjectID,
                                    ProjectCode = project.ProjectCode,
                                    CreatorName = project.Creator.UserInfo.FullName,
                                    Title = project.Title,
                                    CurrentFunded = backing.BackingDetail.PledgedAmount,
                                    BackedDate = backing.BackedDate,
                                    Status = project.Status,
                                    BackingId = backing.BackingID,
                                    IsExprired = project.IsExprired
                                }).OrderByDescending(x => x.BackedDate).ToList();
                return Project;
            }

        }

        // 24/10/2015 - MaiCTP - Get BackingInfo
        public List<BackingInfoDTO> BackingInfo(string projectCode)
        {

            using (var db = new DDLDataContext())
            {
                var Backing = (from backing in db.Backings
                               from rewad in db.RewardPkgs
                               from project in db.Projects
                               where project.ProjectCode == projectCode && backing.ProjectID == project.ProjectID && rewad.RewardPkgID == backing.BackingDetail.RewardPkgID
                               select new BackingInfoDTO
                               {

                                   RewadDiscription = rewad.Description,
                                   PledgeAmount = rewad.PledgeAmount,
                                   Quantity = backing.BackingDetail.Quantity,
                                   BackedDate = backing.BackedDate,
                                   TotalAmount = backing.BackingDetail.Quantity * rewad.PledgeAmount,
                                   BackingDiscription = backing.BackingDetail.Description,
                                   ProfileImage = backing.User.UserInfo.ProfileImage,
                                   FullName = backing.User.UserInfo.FullName,
                                   Email = backing.BackingDetail.Email,
                                   Add = backing.User.UserInfo.Address,
                                   Phone = backing.User.UserInfo.PhoneNumber
                               }).Distinct().ToList();
                return Backing;
            }

        }

        // 17/10/2015 - MaiCTP - Get BackedProject
        public List<ProjectBasicViewDTO> GetBackedProject(String userName)
        {

            using (var db = new DDLDataContext())
            {
                var Project = (from backing in db.Backings
                               from project in db.Projects
                               where backing.User.Username == userName && project.ProjectID == backing.ProjectID
                               select new ProjectBasicViewDTO
                               {
                                   ProjectID = project.ProjectID,
                                   ProjectCode = project.ProjectCode,
                                   Title = project.Title,
                                   CreatorName = project.Creator.UserInfo.FullName,
                                   ImageUrl = project.ImageUrl,
                                   SubDescription = project.SubDescription,
                                   Location = project.Location,
                                   CurrentFunded = Decimal.Round((project.CurrentFunded / project.FundingGoal) * 100),
                                   CurrentFundedNumber = project.CurrentFunded,
                                   ExpireDate = DbFunctions.DiffDays(DateTime.Now, project.ExpireDate),
                                   FundingGoal = project.FundingGoal,
                                   Category = project.Category.Name,
                                   CategoryID = project.Category.CategoryID,
                                   Backers = project.Backings.Count,
                                   CreatedDate = project.CreatedDate,
                                   PopularPoint = project.PopularPoint
                               }).Distinct().ToList();


                return Project;

            }

        }

        //18/10/2015 - MaiCTP - Get StarredProject
        public List<ProjectBasicViewDTO> GetStarredProject(String userName)
        {
            using (var db = new DDLDataContext())
            {
                var Project = (from remind in db.Reminds
                               from project in db.Projects
                               where remind.User.Username == userName && project.ProjectID == remind.ProjectID
                               select new ProjectBasicViewDTO
                               {
                                   ProjectID = project.ProjectID,
                                   ProjectCode = project.ProjectCode,
                                   CreatorName = project.Creator.UserInfo.FullName,
                                   Title = project.Title,
                                   ImageUrl = project.ImageUrl,
                                   SubDescription = project.SubDescription,
                                   Location = project.Location,
                                   CurrentFunded = Decimal.Round((project.CurrentFunded / project.FundingGoal) * 100),
                                   CurrentFundedNumber = project.CurrentFunded,
                                   ExpireDate = DbFunctions.DiffDays(DateTime.Now, project.ExpireDate),
                                   FundingGoal = project.FundingGoal,
                                   Category = project.Category.Name,
                                   CategoryID = project.Category.CategoryID,
                                   Backers = project.Backings.Count,
                                   CreatedDate = project.CreatedDate,
                                   PopularPoint = project.PopularPoint
                               }).Distinct().ToList();


                return Project;
            }
        }


        // 18/10/2015 - MaiCTP - Get CreatedProject
        public List<ProjectBasicViewDTO> GetCreatedProject(String userName)
        {
            using (var db = new DDLDataContext())
            {
                var projectList = (from project in db.Projects
                                   where project.Creator.Username.Equals(userName, StringComparison.OrdinalIgnoreCase)
                                   select new ProjectBasicViewDTO
                                   {
                                       ProjectID = project.ProjectID,
                                       ProjectCode = project.ProjectCode,
                                       CreatorName = project.Creator.UserInfo.FullName,
                                       Title = project.Title,
                                       ImageUrl = project.ImageUrl,
                                       SubDescription = project.SubDescription,
                                       Location = project.Location,
                                       CurrentFunded = Decimal.Round((project.CurrentFunded / project.FundingGoal) * 100),
                                       CurrentFundedNumber = project.CurrentFunded,
                                       ExpireDate = DbFunctions.DiffDays(DateTime.Now, project.ExpireDate),
                                       FundingGoal = project.FundingGoal,
                                       Category = project.Category.Name,
                                       CategoryID = project.Category.CategoryID,
                                       Backers = project.Backings.Count,
                                       CreatedDate = project.CreatedDate,
                                       PopularPoint = project.PopularPoint,
                                       Status = project.IsExprired ? DDLConstants.ProjectStatus.EXPIRED : project.Status
                                   }).Distinct();


                return projectList.ToList();
            }

        }

        //21/10/2015- MaiCTP - DeleteProjectReminded
        public bool DeleteProjectReminded(int projectID)
        {

            using (var db = new DDLDataContext())
            {
                var deleteProjectReminded = db.Reminds.FirstOrDefault(x => x.ProjectID == projectID);

                if (deleteProjectReminded == null)
                {
                    throw new KeyNotFoundException();
                }

                db.Reminds.Remove(deleteProjectReminded);
                db.SaveChanges();

                return true;
            }
        }

        //24/10/2015- MaiCTP - DeleteProjectDraft
        public bool DeleteProjectDraft(int projectID)
        {

            using (var db = new DDLDataContext())
            {
                var deleteProjectDraft = db.Projects.FirstOrDefault(x => x.ProjectID == projectID && (x.Status == DDLConstants.ProjectStatus.DRAFT
                                                                                                        || x.Status == DDLConstants.ProjectStatus.REJECTED));

                if (deleteProjectDraft == null)
                {
                    throw new KeyNotFoundException();
                }

                db.Projects.Remove(deleteProjectDraft);
                db.SaveChanges();

                return true;
            }
        }


        public bool RemindProject(string userName, string projectCode)
        {
            using (var db = new DDLDataContext())
            {
                var userCurrent = db.DDL_Users.FirstOrDefault(x => x.Username.Equals(userName));
                var project = db.Projects.FirstOrDefault(x => x.ProjectCode == projectCode);
                var reminded =
                    db.Reminds.FirstOrDefault(
                        x => x.UserID.Equals(userCurrent.DDL_UserID) && x.ProjectID == project.ProjectID);
                if (reminded != null)
                {
                    db.Reminds.Remove(reminded);
                    db.SaveChanges();

                    ProjectRepository.Instance.CaculateProjectPoint(projectCode, DDLConstants.PopularPointType.RemoveRemindPoint);
                    return false;
                }
                else
                {
                    reminded = new Remind
                    {
                        Project = project,
                        User = userCurrent,
                        RemindID = 0,
                        ProjectID = 0,
                        UserID = 0
                    };
                    reminded.Project.ProjectID = project.ProjectID;
                    reminded.User.DDL_UserID = userCurrent.DDL_UserID;
                    db.Reminds.Add(reminded);
                    db.SaveChanges();

                    ProjectRepository.Instance.CaculateProjectPoint(projectCode, DDLConstants.PopularPointType.RemindPoint);
                }
                return true;
            }
        }




        public BackingDTO GetListBacker(string projectCode)
        {
            using (var db = new DDLDataContext())
            {
                var project = db.Projects.FirstOrDefault(x => x.ProjectCode == projectCode);
                // Check project exist?
                if (project == null)
                {
                    throw new KeyNotFoundException();
                }
                project.CreatedDate = CommonUtils.ConvertDateTimeFromUtc(project.CreatedDate);
                var list = new BackingDTO();
                // Get linq.
                var listBackerLinQ = from backer in db.Backings
                                     where project.ProjectID == backer.ProjectID
                                     orderby backer.BackedDate descending
                                     select new BackerDTO
                                     {
                                         Amount = backer.BackingDetail.PledgedAmount,
                                         Date = SqlFunctions.DateAdd("hh", 7, backer.BackedDate),
                                         FullName = backer.User.UserInfo.FullName,
                                         BackingId = backer.BackingID,
                                     };
                // Generate all date from created date 1 days to now
                var dateMonths = Enumerable.Range(0, 1 + CommonUtils.DateTimeNowGMT7().Date.Subtract(project.CreatedDate.AddDays(-1).Date).Days)
                    .Select(offset =>
                    {
                        var date = project.CreatedDate.AddDays(-1).AddDays(offset); return new { Year = date.Year, Month = date.Month, Day = date.Day };
                    });
                // Create chart.
                //var query = listBackerLinQ.Where(x => x.Date.HasValue)
                //  .GroupBy(x => new { x.Date.Value.Day, x.Date.Value.Month })
                //  .Select(grp => new
                //  {
                //      Day = grp.Key.Day,
                //      Month = grp.Key.Month,
                //      Total = grp.Sum(x => x.Amount)
                //  }).ToList();

                // Join with data for each day.
                var datetimeNow = CommonUtils.DateTimeNowGMT7();
                var query = (from dateMonth in dateMonths
                             join backing in
                                 (from backing in listBackerLinQ
                                  where backing.Date >= project.CreatedDate && backing.Date <= datetimeNow && backing.Date.HasValue
                                  group backing by new { Year = backing.Date.Value.Year, Month = backing.Date.Value.Month, Day = backing.Date.Value.Day } into g
                                  select new { Key = g.Key, Total = g.Sum(x => x.Amount) }) on dateMonth equals backing.Key into j
                             from k in j.DefaultIfEmpty()
                             select new { Year = dateMonth.Year, Day = dateMonth.Day, Month = dateMonth.Month, Total = k != null ? k.Total : 0 }).ToList();


                var listAmount = new List<SumAmount>();


                // Sum data for each day
                decimal total = 0;
                for (var i = 0; i < query.Count; i++)
                {
                    total += query[i].Total;
                    var sum = new SumAmount
                    {
                        Amount = total,
                        Day = query[i].Day,
                        Month = query[i].Month,
                        Year = query[i].Year,
                    };
                    listAmount.Add(sum);
                }

                // Create table.
                var listBacker = listBackerLinQ.ToList();

                //foreach (var backer in listBackerLinQ)
                //{
                //    backer.Date = CommonUtils.ConvertDateTimeFromUtc(backer.Date.GetValueOrDefault());
                //    listBacker.Add(backer);
                //}
                list.sumAmount = listAmount;
                list.listBacker = listBacker;
                list.Amount = new List<decimal>();
                list.Date = new List<string>();
                foreach (var item in listAmount)
                {
                    var date = item.Day.ToString() + '/' + item.Month + "/" + item.Year;
                    list.Date.Add(date);
                    list.Amount.Add(item.Amount);
                }
                return list;
            }
        }

        #region Comment

        private List<CommentDTO> GetComments(int projectID, bool showHide, DateTime? lastDateTime)
        {
            using (var db = new DDLDataContext())
            {
                var commentsQuery = from comment in db.Comments
                                    where comment.ProjectID == projectID
                                    orderby comment.CreatedDate descending
                                    select new CommentDTO
                                    {
                                        FullName = comment.User.UserInfo.FullName,
                                        ProfileImage = comment.User.UserInfo.ProfileImage,
                                        CreatedDate = comment.CreatedDate,
                                        UserName = comment.User.Username,
                                        CommentContent = comment.CommentContent,
                                        CommentID = comment.CommentID,
                                        IsHide = comment.IsHide,
                                        IsEdited = comment.IsEdited
                                    };

                if (!showHide)
                {
                    commentsQuery = commentsQuery.Where(x => !x.IsHide);
                }

                var lastDateTimeUtc = CommonUtils.ConvertDateTimeToUtc(lastDateTime.GetValueOrDefault());

                if (lastDateTime != null)
                {
                    commentsQuery = commentsQuery.Where(x => x.CreatedDate < lastDateTimeUtc);
                }

                //var commentsList = showHide ? commentsQuery.Take(10).ToList() : commentsQuery.Where(x => !x.IsHide).Take(10).ToList();
                var commentsList = commentsQuery.Take(10).ToList();

                commentsList.ForEach(x => x.CreatedDate = CommonUtils.ConvertDateTimeFromUtc(x.CreatedDate));

                return commentsList;
            }
        }

        public List<CommentDTO> AddComment(string projectCode, CommentDTO comment, DateTime lastCommentDateTime)
        {
            using (var db = new DDLDataContext())
            {
                // Check user exist.
                var user = UserRepository.Instance.GetByUserNameOrEmail(comment.UserName);
                if (user == null)
                {
                    throw new UserNotFoundException();
                }

                // Get project
                Project project = null;
                if (projectCode != null)
                {
                    // Check conversation exist.
                    project =
                        db.Projects.FirstOrDefault(
                            c => c.ProjectCode.Equals(projectCode, StringComparison.OrdinalIgnoreCase));
                }
                if (project == null)
                {
                    throw new KeyNotFoundException();
                }

                var projectCreator = project.Creator.Username;

                // Create comment.
                var newComment = db.Comments.Create();
                newComment.CommentContent = comment.CommentContent;
                newComment.CreatedDate = DateTime.UtcNow;
                newComment.IsHide = false;
                newComment.UserID = user.DDL_UserID;
                newComment.ProjectID = project.ProjectID;
                newComment.IsEdited = false;
                newComment.UpdatedDate = DateTime.UtcNow;


                // Add to DB.
                db.Comments.Add(newComment);
                db.SaveChanges();

                var lastDatimeTimeUtc = CommonUtils.ConvertDateTimeToUtc(lastCommentDateTime);

                // Get list new comment.
                var commentsQuery = from commentItem in db.Comments
                                    where commentItem.Project.ProjectCode == projectCode && commentItem.CreatedDate > lastDatimeTimeUtc
                                    orderby commentItem.CreatedDate descending
                                    select new CommentDTO
                                    {
                                        IsHide = commentItem.IsHide,
                                        FullName = commentItem.User.UserInfo.FullName,
                                        CreatedDate = commentItem.CreatedDate,
                                        ProfileImage = commentItem.User.UserInfo.ProfileImage,
                                        UserName = commentItem.User.Username,
                                        CommentContent = commentItem.CommentContent,
                                        CommentID = commentItem.CommentID,
                                    };

                var commentsList = (comment.UserName == projectCreator)
                    ? commentsQuery.ToList()
                    : commentsQuery.Where(c => !c.IsHide).ToList();

                commentsList.ForEach(x => x.CreatedDate = CommonUtils.ConvertDateTimeFromUtc(x.CreatedDate));

                if (projectCreator != comment.UserName)
                {
                    CaculateProjectPoint(projectCode, DDLConstants.PopularPointType.CommentPoint);
                }

                return commentsList;
            }
        }

        /// <summary>
        /// ShowHideComment function
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns>CommentDTO</returns>
        public CommentDTO ShowHideComment(int id, string userName)
        {
            using (var db = new DDLDataContext())
            {
                // Get comment.
                var comment = db.Comments.FirstOrDefault(c => c.CommentID == id);
                if (comment == null)
                {
                    throw new KeyNotFoundException();
                }

                // Check creator.
                if (!comment.Project.Creator.Username.Equals(userName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new NotPermissionException();
                }

                // Change hide status
                comment.IsHide = !comment.IsHide;

                // Save in DB
                db.SaveChanges();

                // Map DTO
                var commentDto = new CommentDTO
                {
                    IsHide = comment.IsHide,
                    FullName = comment.User.UserInfo.FullName,
                    CreatedDate = CommonUtils.ConvertDateTimeFromUtc(comment.CreatedDate),
                    ProfileImage = comment.User.UserInfo.ProfileImage,
                    UserName = comment.User.Username,
                    CommentContent = comment.CommentContent,
                    CommentID = comment.CommentID,
                    IsEdited = comment.IsEdited
                };

                return commentDto;
            }
        }

        /// <summary>
        /// EditComment
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userName"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public CommentDTO EditComment(int id, string userName, string content)
        {
            using (var db = new DDLDataContext())
            {
                // Get comment.
                var comment = db.Comments.FirstOrDefault(c => c.CommentID == id);
                if (comment == null)
                {
                    throw new KeyNotFoundException();
                }

                // Check permission.
                if (!comment.User.Username.Equals(userName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new NotPermissionException();
                }

                // Change content
                comment.CommentContent = content;
                comment.UpdatedDate = DateTime.UtcNow;
                comment.IsEdited = true;

                // Save in DB
                db.SaveChanges();

                // Map DTO
                var commentDto = new CommentDTO
                {
                    IsHide = comment.IsHide,
                    FullName = comment.User.UserInfo.FullName,
                    CreatedDate = CommonUtils.ConvertDateTimeFromUtc(comment.CreatedDate),
                    ProfileImage = comment.User.UserInfo.ProfileImage,
                    UserName = comment.User.Username,
                    CommentContent = comment.CommentContent,
                    CommentID = comment.CommentID,
                    IsEdited = comment.IsEdited
                };

                return commentDto;
            }
        }

        /// <summary>
        /// DeleteComment
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userName"></param>
        /// <returns>boolean</returns>
        public bool DeleteComment(int id, string userName)
        {
            using (var db = new DDLDataContext())
            {
                // Get comment.
                var comment = db.Comments.FirstOrDefault(c => c.CommentID == id);
                if (comment == null)
                {
                    throw new KeyNotFoundException();
                }

                // Check creator.
                if (!comment.User.Username.Equals(userName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new NotPermissionException();
                }

                // Delete comment
                db.Comments.Remove(comment);

                // Save in DB
                db.SaveChanges();

                return true;
            }
        }
        #endregion

        #endregion
    }
}